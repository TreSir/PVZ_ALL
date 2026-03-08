using System.Collections.Generic;
using UnityEngine;
using ObjectPool;
using GameBase;
using Config;
using SourceLoad;

namespace UI
{
    public class BaseUIManager : IGameSystem
    {
        private static BaseUIManager _instance;
        public static BaseUIManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new BaseUIManager();
                }
                return _instance;
            }
        }

        public int Priority => 2000;

        private const string ConfigPath = "Configs/GameSettingConfig";

        private Dictionary<string, string> _pathDict;
        private Dictionary<string, GameObject> _prefabDict;
        private Dictionary<string, BasePanel> _panelDict;
        private Dictionary<string, Queue<BasePanel>> _panelPool;

        public IReadOnlyDictionary<string, BasePanel> PanelDict => _panelDict;
        private Transform _uiRoot;
        private GameObject _panelPoolRoot;
        private bool _usePool = true;
        private int _initialPoolSize = 5;
        private bool _isInitialized;

        public Transform UIRoot
        {
            get
            {
                if (_uiRoot == null)
                {
                    var canvas = GameObject.Find("Canvas");
                    if (canvas != null)
                    {
                        _uiRoot = canvas.transform;
                    }
                    else
                    {
                        _uiRoot = new GameObject("Canvas").transform;
                    }
                }
                return _uiRoot;
            }
        }

        public int sortingOrder = 1;

        public BaseUIManager()
        {
            if (_instance != null && _instance != this)
            {
                return;
            }
            _instance = this;
            InitDics();
        }

        private void InitDics()
        {
            _prefabDict = new Dictionary<string, GameObject>();
            _panelDict = new Dictionary<string, BasePanel>();
            _panelPool = new Dictionary<string, Queue<BasePanel>>();
            _pathDict = new Dictionary<string, string>()
            {
                {UIConst.MainMenuPanel, "MainMenuPanel"}
            };
        }

        public void SetUsePool(bool usePool, int initialPoolSize = 5)
        {
            _usePool = usePool;
            _initialPoolSize = initialPoolSize;
        }

        #region IGameSystem Implementation

        public void Initialize()
        {
            if (_isInitialized)
            {
                Debug.LogWarning("[BaseUIManager] Already initialized");
                return;
            }

            Debug.Log("[BaseUIManager] Initializing...");

            var config = ResourceManager.Load<GameSettingConfig>(ConfigPath);
            if (config == null)
            {
                Debug.LogWarning($"[BaseUIManager] Config not found at Resources/{ConfigPath}");
                OpenDefaultPanel();
                _isInitialized = true;
                return;
            }

            if (config.ui.enablePanelPool)
            {
                _usePool = true;
                _initialPoolSize = config.ui.panelPoolInitialSize;
            }

            if (!string.IsNullOrEmpty(config.ui.startupPanelName))
            {
                OpenStartupPanel(config.ui.startupPanelName);
            }

            _isInitialized = true;
            Debug.Log("[BaseUIManager] Initialized successfully");
        }

        public void Shutdown()
        {
            Debug.Log("[BaseUIManager] Shutting down...");
            
            if (!Application.isPlaying)
            {
                _panelDict?.Clear();
                _panelPool?.Clear();
                _panelPoolRoot = null;
                _isInitialized = false;
                return;
            }
            
            CloseAllPanel();
            ClearPool();
            _isInitialized = false;
        }

        private void OpenStartupPanel(string panelName)
        {
            var panel = OpenPanel(panelName);
            if (panel != null)
            {
                Debug.Log($"[BaseUIManager] Opened startup panel: {panelName}");
            }
            else
            {
                Debug.LogWarning($"[BaseUIManager] Failed to open startup panel: {panelName}");
                OpenDefaultPanel();
            }
        }

        private void OpenDefaultPanel()
        {
            var panel = OpenPanel(UIConst.MainMenuPanel);
            if (panel != null)
            {
                Debug.Log($"[BaseUIManager] Opened default panel: {UIConst.MainMenuPanel}");
            }
        }

        #endregion

        #region Panel Management

        public BasePanel OpenPanel(string panelName)
        {
            BasePanel panel = null;

            if (_panelDict.TryGetValue(panelName, out panel))
            {
                Debug.Log($"[BaseUIManager] Panel {panelName} is already opened");
                panel.SetActive(true);
                return panel;
            }

            if (_usePool && _panelPool.TryGetValue(panelName, out var pool) && pool.Count > 0)
            {
                panel = pool.Dequeue();
                _panelDict[panelName] = panel;
                panel.transform.SetParent(UIRoot, false);
                panel.OpenPanel();
                UpdatePanelSortingOrder(panel.gameObject);
                return panel;
            }

            string path = null;
            if (!_pathDict.TryGetValue(panelName, out path))
            {
                Debug.LogWarning($"[BaseUIManager] Panel path not found: {panelName}");
                return null;
            }

            GameObject prefab = null;
            if (!_prefabDict.TryGetValue(panelName, out prefab))
            {
                prefab = Resources.Load<GameObject>("Prefabs/UI/" + path);
                if (prefab == null)
                {
                    Debug.LogWarning($"[BaseUIManager] Panel prefab not found: Prefabs/UI/{path}");
                    return null;
                }
                _prefabDict[panelName] = prefab;
            }

            GameObject panelObject = UnityEngine.Object.Instantiate(prefab, UIRoot, false);
            panel = panelObject.GetComponent<BasePanel>();
            if (panel == null)
            {
                Debug.LogWarning($"[BaseUIManager] Panel {panelName} does not have BasePanel component");
                UnityEngine.Object.Destroy(panelObject);
                return null;
            }

            _panelDict[panelName] = panel;
            panel.OpenPanel();
            UpdatePanelSortingOrder(panelObject);

            return panel;
        }

        private void UpdatePanelSortingOrder(GameObject panelObject)
        {
            var canvas = panelObject.GetComponent<UnityEngine.Canvas>();
            if (canvas != null)
            {
                canvas.sortingOrder = sortingOrder;
                sortingOrder++;
            }
        }

        public bool ClosePanel(string panelName, bool wantRemove = true)
        {
            if (!_panelDict.TryGetValue(panelName, out var panel))
            {
                Debug.LogWarning($"[BaseUIManager] Panel not found: {panelName}");
                return false;
            }

            if (panel == null)
            {
                _panelDict.Remove(panelName);
                return false;
            }

            if (wantRemove)
            {
                if (_usePool && panel.gameObject != null)
                {
                    ReturnToPool(panelName, panel);
                }
                else if (panel.gameObject != null)
                {
                    panel.ClosePanel();
                }
            }

            _panelDict.Remove(panelName);
            return true;
        }

        private void ReturnToPool(string panelName, BasePanel panel)
        {
            if (panel == null || panel.gameObject == null)
            {
                return;
            }

            panel.ClosePanel();

            if (!_panelPool.TryGetValue(panelName, out var pool))
            {
                pool = new Queue<BasePanel>();
                _panelPool[panelName] = pool;
            }

            if (_panelPoolRoot == null)
            {
                _panelPoolRoot = new GameObject("[UI_Panel_Pool]");
                UnityEngine.Object.DontDestroyOnLoad(_panelPoolRoot);
            }

            panel.transform.SetParent(_panelPoolRoot.transform, false);
            panel.gameObject.SetActive(false);
            pool.Enqueue(panel);

            Debug.Log($"[BaseUIManager] Panel {panelName} returned to pool. Pool size: {pool.Count}");
        }

        public void CloseAllPanel(bool wantRemove = true)
        {
            if (_panelDict == null || _panelDict.Count == 0)
                return;

            var panelNames = new List<string>(_panelDict.Keys);
            foreach (var panelName in panelNames)
            {
                if (_panelDict.TryGetValue(panelName, out var panel))
                {
                    if (panel != null && panel.gameObject != null)
                    {
                        ClosePanel(panelName, wantRemove);
                    }
                    else
                    {
                        _panelDict.Remove(panelName);
                    }
                }
            }
        }

        public void OnPanelClosed(string panelName)
        {
        }

        public bool IsPanelOpened(string panelName)
        {
            return _panelDict.ContainsKey(panelName);
        }

        public BasePanel GetPanel(string panelName)
        {
            if (_panelDict.TryGetValue(panelName, out var panel))
            {
                return panel;
            }
            return null;
        }

        public void PreloadPanel(string panelName)
        {
            if (!_pathDict.ContainsKey(panelName))
            {
                Debug.LogWarning($"[BaseUIManager] Panel path not found for preload: {panelName}");
                return;
            }

            if (!_prefabDict.TryGetValue(panelName, out var prefab))
            {
                var path = _pathDict[panelName];
                prefab = Resources.Load<GameObject>("Prefabs/UI/" + path);
                if (prefab != null)
                {
                    _prefabDict[panelName] = prefab;
                }
            }

            if (!_panelPool.ContainsKey(panelName))
            {
                _panelPool[panelName] = new Queue<BasePanel>();
            }

            var pool = _panelPool[panelName];
            for (int i = 0; i < _initialPoolSize; i++)
            {
                var panelObj = UnityEngine.Object.Instantiate(prefab, _panelPoolRoot != null ? _panelPoolRoot.transform : null, false);
                var panel = panelObj.GetComponent<BasePanel>();
                if (panel != null)
                {
                    panelObj.SetActive(false);
                    pool.Enqueue(panel);
                }
                else
                {
                    UnityEngine.Object.Destroy(panelObj);
                }
            }

            Debug.Log($"[BaseUIManager] Preloaded {panelName}, count: {pool.Count}");
        }

        public void ClearPool()
        {
            foreach (var pool in _panelPool.Values)
            {
                pool.Clear();
            }
            _panelPool.Clear();

            if (_panelPoolRoot != null)
            {
                var go = _panelPoolRoot;
                _panelPoolRoot = null;
                
                if (go != null && !go.Equals(null) && Application.isPlaying)
                {
                    UnityEngine.Object.Destroy(go);
                }
            }
        }

        #endregion
    }

    public class UIConst
    {
        public const string MainMenuPanel = "MainMenuPanel";
    }
}
