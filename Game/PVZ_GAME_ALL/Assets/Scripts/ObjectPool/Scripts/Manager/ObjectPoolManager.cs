using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ObjectPool
{
    public class PoolInfo
    {
        public string Key;
        public int Count;
        public int ActiveCount;
        public int Capacity;
        public Type ObjectType;
        public bool IsGameObjectPool;
    }

    public class ObjectPoolManager : MonoBehaviour
    {
        private static ObjectPoolManager _instance;
        private static readonly object _lock = new object();
        private static bool _applicationIsQuitting;

        public static ObjectPoolManager Instance
        {
            get
            {
                if (_applicationIsQuitting)
                {
                    Debug.LogWarning("[ObjectPoolManager] Application is quitting, returning null.");
                    return null;
                }

                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = FindObjectOfType<ObjectPoolManager>();
                        
                        if (_instance == null)
                        {
                            var go = new GameObject("[ObjectPoolManager]");
                            _instance = go.AddComponent<ObjectPoolManager>();
                            DontDestroyOnLoad(go);
                        }
                    }
                    
                    return _instance;
                }
            }
        }
        //2.25阅读代码，了解ObjectPoolManager的工作原理
        //TODO：NEXT
        public static bool HasInstance => _instance != null && !_applicationIsQuitting;

        private readonly Dictionary<string, object> _pools = new Dictionary<string, object>();
        private readonly Dictionary<string, PoolConfig> _poolConfigs = new Dictionary<string, PoolConfig>();
        private Transform _poolRoot;

        public int TotalPoolCount => _pools.Count;

        public int TotalActiveObjectCount
        {
            get
            {
                int count = 0;
                foreach (var kvp in _pools)
                {
                    if (kvp.Value is GameObjectPool goPool)
                    {
                        count += goPool.ActiveCount;
                    }
                }
                return count;
            }
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            _instance = this;
            DontDestroyOnLoad(gameObject);
            
            _poolRoot = new GameObject("[Pools]").transform;
            _poolRoot.SetParent(transform);
            
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                SceneManager.sceneLoaded -= OnSceneLoaded;
                ClearAllPools();
                _applicationIsQuitting = true;
            }
        }

        private void OnApplicationQuit()
        {
            _applicationIsQuitting = true;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            foreach (var kvp in _poolConfigs)
            {
                if (kvp.Value.autoReleaseOnSceneChange)
                {
                    if (_pools.TryGetValue(kvp.Key, out var pool) && pool is GameObjectPool goPool)
                    {
                        goPool.ReleaseAll();
                    }
                }
            }
        }

        #region GameObject Pool

        public GameObjectPool GetOrCreatePool(string key, GameObject prefab, PoolConfig config = null)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));
            
            if (prefab == null)
                throw new ArgumentNullException(nameof(prefab));

            if (_pools.TryGetValue(key, out var existingPool))
            {
                if (existingPool is GameObjectPool goPool)
                {
                    return goPool;
                }
                throw new InvalidOperationException($"Pool with key '{key}' exists but is not a GameObjectPool");
            }

            config = config ?? PoolConfig.Default;
            var parent = new GameObject($"[Pool_{key}]").transform;
            parent.SetParent(_poolRoot);
            
            var pool = new GameObjectPool(prefab, parent, config);
            
            if (config.initialSize > 0)
            {
                pool.Preload(config.initialSize);
            }
            
            _pools[key] = pool;
            _poolConfigs[key] = config;
            
            PoolEvents.InvokePoolCreated(key, pool.Count);
            
            return pool;
        }

        public GameObjectPool GetPool(string key)
        {
            if (_pools.TryGetValue(key, out var pool) && pool is GameObjectPool goPool)
            {
                return goPool;
            }
            return null;
        }

        #endregion

        #region Generic Object Pool

        public ObjectPool<T> GetOrCreatePool<T>(string key, PoolConfig config = null) where T : class, new()
        {
            return GetOrCreatePool(key, () => new T(), null, null, null, config);
        }

        public ObjectPool<T> GetOrCreatePool<T>(
            string key,
            Func<T> createFunc,
            Action<T> actionOnGet = null,
            Action<T> actionOnRelease = null,
            Action<T> actionOnDestroy = null,
            PoolConfig config = null) where T : class
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            if (_pools.TryGetValue(key, out var existingPool))
            {
                if (existingPool is ObjectPool<T> typedPool)
                {
                    return typedPool;
                }
                throw new InvalidOperationException($"Pool with key '{key}' exists but is not an ObjectPool<{typeof(T).Name}>");
            }

            config = config ?? PoolConfig.Default;
            
            var pool = new ObjectPool<T>(createFunc, actionOnGet, actionOnRelease, actionOnDestroy, config);
            
            if (config.initialSize > 0)
            {
                pool.Preload(config.initialSize);
            }
            
            _pools[key] = pool;
            _poolConfigs[key] = config;
            
            PoolEvents.InvokePoolCreated(key, pool.Count);
            
            return pool;
        }

        public ObjectPool<T> GetPool<T>(string key) where T : class
        {
            if (_pools.TryGetValue(key, out var pool) && pool is ObjectPool<T> typedPool)
            {
                return typedPool;
            }
            return null;
        }

        #endregion

        #region Pool Management

        public bool HasPool(string key)
        {
            return _pools.ContainsKey(key);
        }

        public void RemovePool(string key)
        {
            if (_pools.TryGetValue(key, out var pool))
            {
                if (pool is IDisposable disposable)
                {
                    disposable.Dispose();
                }
                
                _pools.Remove(key);
                _poolConfigs.Remove(key);
                
                PoolEvents.InvokePoolRemoved(key);
            }
        }

        public void ClearAllPools()
        {
            foreach (var kvp in _pools)
            {
                if (kvp.Value is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }
            
            _pools.Clear();
            _poolConfigs.Clear();
            
            PoolEvents.InvokeAllPoolsCleared();
        }

        public void Preload(string key, int count)
        {
            if (_pools.TryGetValue(key, out var pool))
            {
                if (pool is GameObjectPool goPool)
                {
                    goPool.Preload(count);
                }
                else if (pool is IPool<object> genericPool)
                {
                    genericPool.Preload(count);
                }
            }
        }

        #endregion

        #region Quick Access

        public GameObject Spawn(string key, GameObject prefab, Vector3 position = default, Quaternion rotation = default)
        {
            var pool = GetOrCreatePool(key, prefab);
            return pool.Get(position, rotation);
        }

        public void Despawn(string key, GameObject obj)
        {
            var pool = GetPool(key);
            if (pool != null)
            {
                pool.Release(obj);
            }
            else
            {
                Debug.LogWarning($"[ObjectPoolManager] No pool found with key '{key}'");
            }
        }

        #endregion

        #region Info & Debug

        public List<PoolInfo> GetAllPoolInfo()
        {
            var list = new List<PoolInfo>();
            
            foreach (var kvp in _pools)
            {
                var info = new PoolInfo
                {
                    Key = kvp.Key
                };
                
                if (kvp.Value is GameObjectPool goPool)
                {
                    info.Count = goPool.Count;
                    info.ActiveCount = goPool.ActiveCount;
                    info.Capacity = goPool.Capacity;
                    info.ObjectType = typeof(GameObject);
                    info.IsGameObjectPool = true;
                }
                else if (kvp.Value.GetType().IsGenericType)
                {
                    var poolType = kvp.Value.GetType();
                    var genericArgs = poolType.GetGenericArguments();
                    if (genericArgs.Length > 0)
                    {
                        info.ObjectType = genericArgs[0];
                    }
                    
                    var countProp = poolType.GetProperty("Count");
                    var activeCountProp = poolType.GetProperty("ActiveCount");
                    var capacityProp = poolType.GetProperty("Capacity");
                    
                    info.Count = countProp != null ? (int)countProp.GetValue(kvp.Value) : 0;
                    info.ActiveCount = activeCountProp != null ? (int)activeCountProp.GetValue(kvp.Value) : 0;
                    info.Capacity = capacityProp != null ? (int)capacityProp.GetValue(kvp.Value) : 0;
                    info.IsGameObjectPool = false;
                }
                
                list.Add(info);
            }
            
            return list;
        }

        #endregion
    }
}
