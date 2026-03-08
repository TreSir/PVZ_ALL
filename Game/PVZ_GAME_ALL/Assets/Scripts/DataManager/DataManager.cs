using System.Collections.Generic;
using UnityEngine;
using GameBase;
using Config;

namespace DataManager
{
    public class DataManager : IGameSystem
    {
        private static DataManager _instance;
        private static readonly object _lock = new object();
        private static bool _applicationIsQuitting;

        public static DataManager Instance
        {
            get
            {
                if (_applicationIsQuitting)
                {
                    Debug.LogWarning("[DataManager] Application is quitting, returning null.");
                    return null;
                }

                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new DataManager();
                    }
                    return _instance;
                }
            }
        }

        public static bool HasInstance => _instance != null && !_applicationIsQuitting;

        public int Priority => 50;

        private const string ConfigPath = "Configs/GameSettingConfig";

        private Dictionary<string, object> _configCache;
        private Dictionary<string, object> _runtimeData;
        private DataSettings _settings;
        private bool _isInitialized;

        public DataManager()
        {
            if (_instance != null && _instance != this)
            {
                return;
            }
            _instance = this;
            _configCache = new Dictionary<string, object>();
            _runtimeData = new Dictionary<string, object>();
        }

        public void Initialize()
        {
            if (_isInitialized)
            {
                Debug.LogWarning("[DataManager] Already initialized");
                return;
            }

            Debug.Log("[DataManager] Initializing...");

            LoadSettings();

            _isInitialized = true;
            Debug.Log("[DataManager] Initialized successfully");
        }

        public void Shutdown()
        {
            Debug.Log("[DataManager] Shutting down...");
            _configCache.Clear();
            _runtimeData.Clear();
            _isInitialized = false;
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
                Debug.LogWarning($"[DataManager] Config not found at Resources/{ConfigPath}, using default settings");
            }
        }

        #region Config Data Loading

        /*
         * ═══════════════════════════════════════════════════════════════════════════
         * 游戏配置加载示例
         * ═══════════════════════════════════════════════════════════════════════════
         *
         * 【目录结构】
         * Assets/
         * └── Resources/
         *     └── Configs/
         *         ├── ZombieConfig.asset      ← 僵尸配置
         *         ├── PlantConfig.asset       ← 植物配置
         *         └── LevelConfig.asset       ← 关卡配置
         *
         * 【使用示例】
         *
         *   // 加载僵尸配置
         *   var zombieConfig = DataManager.Instance.LoadConfig<ZombieConfig>("Configs/ZombieConfig");
         *   foreach (var zombie in zombieConfig.zombies)
         *   {
         *       Debug.Log($"僵尸: {zombie.name}, 血量: {zombie.health}");
         *   }
         *
         *   // 加载植物配置
         *   var plantConfig = DataManager.Instance.LoadConfig<PlantConfig>("Configs/PlantConfig");
         *
         *   // 加载关卡配置
         *   var levelConfig = DataManager.Instance.LoadConfig<LevelConfig>("Configs/LevelConfig");
         *
         * 【注意事项】
         * - 配置文件必须放在 Resources 目录下
         * - 路径不需要写 "Resources/" 前缀
         * - 路径不需要写 ".asset" 后缀
         * - 首次加载后自动缓存，再次加载直接返回缓存
         *
         * ═══════════════════════════════════════════════════════════════════════════
         */

        public T LoadConfig<T>(string path) where T : Object
        {
            string cacheKey = $"Config:{path}";

            if (_settings != null && _settings.enableCache && _configCache.TryGetValue(cacheKey, out var cached))
            {
                return cached as T;
            }

            T asset = Resources.Load<T>(path);
            if (asset == null)
            {
                Debug.LogWarning($"[DataManager] Failed to load config at: {path}");
                return null;
            }

            if (_settings != null && _settings.enableCache)
            {
                _configCache[cacheKey] = asset;
            }

            return asset;
        }

        public void ClearConfigCache()
        {
            _configCache.Clear();
            Debug.Log("[DataManager] Config cache cleared");
        }

        #endregion

        #region Runtime Data Management

        public void Register<T>(string key, T data)
        {
            if (string.IsNullOrEmpty(key))
            {
                Debug.LogWarning("[DataManager] Key cannot be null or empty");
                return;
            }

            _runtimeData[key] = data;
        }

        public T Get<T>(string key)
        {
            if (_runtimeData.TryGetValue(key, out var data))
            {
                return (T)data;
            }
            return default;
        }

        public bool TryGet<T>(string key, out T data)
        {
            if (_runtimeData.TryGetValue(key, out var cached))
            {
                data = (T)cached;
                return true;
            }
            data = default;
            return false;
        }

        public void Unregister(string key)
        {
            if (_runtimeData.ContainsKey(key))
            {
                _runtimeData.Remove(key);
            }
        }

        public bool Contains(string key)
        {
            return _runtimeData.ContainsKey(key);
        }

        public void ClearRuntimeData()
        {
            _runtimeData.Clear();
            Debug.Log("[DataManager] Runtime data cleared");
        }

        #endregion

        #region Level Temporary Data (Design Notes - TODO)

        /*
         * ═══════════════════════════════════════════════════════════════════════════
         * 关卡临时数据管理 - 设计思路
         * ═══════════════════════════════════════════════════════════════════════════
         *
         * 【设计原则】
         * DataManager 统一管理所有运行时数据，关卡控制器只负责驱动生命周期
         *
         * 【使用示例】
         *
         *   // 进入关卡 - 注册临时数据
         *   DataManager.Instance.Register("LevelData", new LevelTempData { levelId = levelId });
         *
         *   // 获取临时数据
         *   var levelData = DataManager.Instance.Get<LevelTempData>("LevelData");
         *   levelData.currentSun += 50;
         *
         *   // 退出关卡 - 移除临时数据
         *   DataManager.Instance.Unregister("LevelData");
         *
         * ═══════════════════════════════════════════════════════════════════════════
         */

        #endregion
    }
}
