using System.Collections.Generic;
using System.IO;
using UnityEngine;
using GameBase;
using Config;

namespace DataManager
{
    public class SaveManager : IGameSystem, IGameSystemUpdate
    {
        private static SaveManager _instance;
        private static readonly object _lock = new object();
        private static bool _applicationIsQuitting;

        public static SaveManager Instance
        {
            get
            {
                if (_applicationIsQuitting)
                {
                    Debug.LogWarning("[SaveManager] Application is quitting, returning null.");
                    return null;
                }

                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new SaveManager();
                    }
                    return _instance;
                }
            }
        }

        public static bool HasInstance => _instance != null && !_applicationIsQuitting;

        public int Priority => 51;

        private const string ConfigPath = "Configs/GameSettingConfig";
        private const int MaxSaveSlots = 3;

        private DataSettings _settings;
        private string _persistentDataPath;
        private bool _isInitialized;

        private int _currentSlotIndex = -1;
        private Dictionary<int, SaveSlotInfo> _saveSlotInfos;
        private float _autoSaveTimer;
        private bool _hasUnsavedChanges;

        public int CurrentSlotIndex => _currentSlotIndex;
        public bool HasUnsavedChanges => _hasUnsavedChanges;
        public bool HasValidSave => _currentSlotIndex >= 0 && _saveSlotInfos.ContainsKey(_currentSlotIndex);
        public string PersistentDataPath => _persistentDataPath;

        public event System.Action<int> OnAutoSaveRequested;

        public SaveManager()
        {
            if (_instance != null && _instance != this)
            {
                return;
            }
            _instance = this;
            _saveSlotInfos = new Dictionary<int, SaveSlotInfo>();
        }

        public void Initialize()
        {
            if (_isInitialized)
            {
                Debug.LogWarning("[SaveManager] Already initialized");
                return;
            }

            Debug.Log("[SaveManager] Initializing...");

            LoadSettings();
            InitializePersistentPath();
            LoadAllSaveSlotInfos();

            _isInitialized = true;
            Debug.Log("[SaveManager] Initialized successfully");
        }

        public void Shutdown()
        {
            Debug.Log("[SaveManager] Shutting down...");

            if (_hasUnsavedChanges && _currentSlotIndex >= 0)
            {
                AutoSave();
            }

            _saveSlotInfos.Clear();
            _isInitialized = false;
        }

        public void Update(float deltaTime)
        {
            if (_settings == null || _settings.autoSaveInterval <= 0)
                return;

            if (_currentSlotIndex < 0 || !_hasUnsavedChanges)
                return;

            _autoSaveTimer += deltaTime;

            if (_autoSaveTimer >= _settings.autoSaveInterval)
            {
                Debug.Log($"[SaveManager] Auto save triggered! Timer: {_autoSaveTimer:F1}s >= Interval: {_settings.autoSaveInterval:F1}s, Slot: {_currentSlotIndex}");
                _autoSaveTimer = 0f;
                AutoSave();
            }
        }

        private void LoadSettings()
        {
            var config = Resources.Load<GameSettingConfig>(ConfigPath);
            if (config != null && config.data != null)
            {
                _settings = config.data;
            }
            else
            {
                _settings = new DataSettings();
                Debug.LogWarning($"[SaveManager] Config not found at Resources/{ConfigPath}, using default settings");
            }
        }

        private void InitializePersistentPath()
        {
            string folderName = _settings?.persistentFolderName ?? "GameData";
            _persistentDataPath = Path.Combine(Application.persistentDataPath, folderName);

            if (!Directory.Exists(_persistentDataPath))
            {
                Directory.CreateDirectory(_persistentDataPath);
            }
        }

        #region Save Slot Management

        public List<SaveSlotInfo> GetAllSaveSlotInfos()
        {
            var result = new List<SaveSlotInfo>();
            for (int i = 0; i < MaxSaveSlots; i++)
            {
                if (_saveSlotInfos.TryGetValue(i, out var info))
                {
                    result.Add(info);
                }
                else
                {
                    result.Add(new SaveSlotInfo { slotIndex = i, isEmpty = true });
                }
            }
            return result;
        }

        public SaveSlotInfo GetSaveSlotInfo(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= MaxSaveSlots)
            {
                Debug.LogWarning($"[SaveManager] Invalid slot index: {slotIndex}");
                return null;
            }

            if (_saveSlotInfos.TryGetValue(slotIndex, out var info))
            {
                return info;
            }
            return new SaveSlotInfo { slotIndex = slotIndex, isEmpty = true };
        }

        private void LoadAllSaveSlotInfos()
        {
            _saveSlotInfos.Clear();
            for (int i = 0; i < MaxSaveSlots; i++)
            {
                string infoPath = GetSlotInfoPath(i);
                if (SerializationUtility.FileExists(infoPath))
                {
                    var info = SerializationUtility.LoadFromFile<SaveSlotInfo>(infoPath);
                    if (info != null)
                    {
                        _saveSlotInfos[i] = info;
                    }
                }
            }
        }

        private void SaveSlotInfo(int slotIndex, SaveSlotInfo info)
        {
            string infoPath = GetSlotInfoPath(slotIndex);
            if (SerializationUtility.SaveToFile(info, infoPath))
            {
                _saveSlotInfos[slotIndex] = info;
            }
        }

        #endregion

        #region Save/Load Operations

        public bool SelectSlot(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= MaxSaveSlots)
            {
                Debug.LogWarning($"[SaveManager] Invalid slot index: {slotIndex}");
                return false;
            }

            _currentSlotIndex = slotIndex;
            _hasUnsavedChanges = false;
            _autoSaveTimer = 0f;

            Debug.Log($"[SaveManager] Selected save slot: {slotIndex}");
            return true;
        }

        public T LoadSaveData<T>(int slotIndex) where T : class, ISaveData, new()
        {
            if (slotIndex < 0 || slotIndex >= MaxSaveSlots)
            {
                Debug.LogWarning($"[SaveManager] Invalid slot index: {slotIndex}");
                return null;
            }

            string filePath = GetSaveFilePath(slotIndex, typeof(T).Name);

            if (!SerializationUtility.FileExists(filePath))
            {
                Debug.Log($"[SaveManager] Save file not found for slot {slotIndex}, creating new data");
                return new T();
            }

            var data = SerializationUtility.LoadFromFile<T>(filePath);
            Debug.Log($"[SaveManager] Loaded save data from slot {slotIndex}");
            return data;
        }

        public bool SaveSaveData<T>(int slotIndex, T data) where T : class, ISaveData
        {
            if (slotIndex < 0 || slotIndex >= MaxSaveSlots)
            {
                Debug.LogWarning($"[SaveManager] Invalid slot index: {slotIndex}");
                return false;
            }

            if (data == null)
            {
                Debug.LogWarning("[SaveManager] Cannot save null data");
                return false;
            }

            string filePath = GetSaveFilePath(slotIndex, typeof(T).Name);

            data.lastSaveTime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            if (SerializationUtility.SaveToFile(data, filePath))
            {
                var info = new SaveSlotInfo
                {
                    slotIndex = slotIndex,
                    isEmpty = false,
                    lastSaveTime = data.lastSaveTime,
                    playTime = data.playTime
                };
                SaveSlotInfo(slotIndex, info);

                Debug.Log($"[SaveManager] Saved data to slot {slotIndex}");
                return true;
            }

            return false;
        }

        public T GetCurrentSaveData<T>() where T : class, ISaveData, new()
        {
            if (_currentSlotIndex < 0)
            {
                Debug.LogWarning("[SaveManager] No slot selected");
                return new T();
            }
            return LoadSaveData<T>(_currentSlotIndex);
        }

        public bool SaveCurrentData<T>(T data) where T : class, ISaveData
        {
            if (_currentSlotIndex < 0)
            {
                Debug.LogWarning("[SaveManager] No slot selected");
                return false;
            }

            bool success = SaveSaveData(_currentSlotIndex, data);
            if (success)
            {
                _hasUnsavedChanges = false;
            }
            return success;
        }

        public void MarkDirty()
        {
            _hasUnsavedChanges = true;
        }

        public void AutoSave()
        {
            if (_currentSlotIndex < 0)
                return;

            Debug.Log($"[SaveManager] Auto saving to slot {_currentSlotIndex}...");
            OnAutoSaveRequested?.Invoke(_currentSlotIndex);
        }

        public bool DeleteSlot(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= MaxSaveSlots)
            {
                Debug.LogWarning($"[SaveManager] Invalid slot index: {slotIndex}");
                return false;
            }

            string slotDir = GetSlotDirectory(slotIndex);
            if (Directory.Exists(slotDir))
            {
                SerializationUtility.DeleteDirectory(slotDir);
            }

            string infoPath = GetSlotInfoPath(slotIndex);
            SerializationUtility.DeleteFile(infoPath);

            _saveSlotInfos.Remove(slotIndex);

            if (_currentSlotIndex == slotIndex)
            {
                _currentSlotIndex = -1;
                _hasUnsavedChanges = false;
            }

            Debug.Log($"[SaveManager] Deleted slot {slotIndex}");
            return true;
        }

        #endregion

        #region Path Helpers

        private string GetSlotDirectory(int slotIndex)
        {
            return Path.Combine(_persistentDataPath, $"Slot_{slotIndex}");
        }

        private string GetSaveFilePath(int slotIndex, string dataName)
        {
            string dir = GetSlotDirectory(slotIndex);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            return Path.Combine(dir, $"{dataName}.json");
        }

        private string GetSlotInfoPath(int slotIndex)
        {
            return Path.Combine(_persistentDataPath, $"Slot_{slotIndex}_info.json");
        }

        #endregion
    }

    [System.Serializable]
    public class SaveSlotInfo
    {
        public int slotIndex;
        public bool isEmpty = true;
        public string lastSaveTime;
        public float playTime;
    }

    public interface ISaveData
    {
        string lastSaveTime { get; set; }
        float playTime { get; set; }
    }
}
