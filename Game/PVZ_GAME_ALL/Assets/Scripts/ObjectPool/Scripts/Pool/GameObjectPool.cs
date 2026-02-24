using System;
using System.Collections.Generic;
using UnityEngine;

namespace ObjectPool
{
    public class GameObjectPool : IPool<GameObject>, IDisposable
    {
        private readonly Stack<GameObject> _pool;
        private readonly GameObject _prefab;
        private readonly Transform _parent;
        private readonly PoolConfig _config;
        private readonly List<GameObject> _activeObjects;
        private readonly Dictionary<GameObject, Action<GameObject>> _releaseCallbacks;
        private bool _disposed;

        public int Count => _pool.Count;
        
        public int ActiveCount => _activeObjects.Count;
        
        public int Capacity
        {
            get => _config.maxSize;
            set => _config.maxSize = value;
        }

        public GameObject Prefab => _prefab;
        
        public Transform Parent => _parent;

        public GameObjectPool(
            GameObject prefab,
            Transform parent = null,
            PoolConfig config = null)
        {
            if (prefab == null)
                throw new ArgumentNullException(nameof(prefab));
            
            _prefab = prefab;
            _parent = parent;
            _config = config ?? PoolConfig.Default;
            _pool = new Stack<GameObject>(_config.initialSize);
            _activeObjects = new List<GameObject>(_config.initialSize);
            _releaseCallbacks = new Dictionary<GameObject, Action<GameObject>>();
        }

        public GameObject Get()
        {
            return Get(Vector3.zero, Quaternion.identity, null);
        }

        public GameObject Get(Transform parent)
        {
            return Get(Vector3.zero, Quaternion.identity, parent);
        }

        public GameObject Get(Vector3 position, Quaternion rotation)
        {
            return Get(position, rotation, null);
        }

        public GameObject Get(Vector3 position, Quaternion rotation, Transform parent)
        {
            GameObject obj;
            
            if (_pool.Count > 0)
            {
                obj = _pool.Pop();
                obj.transform.position = position;
                obj.transform.rotation = rotation;
                if (parent != null)
                    obj.transform.SetParent(parent);
            }
            else if (_config.autoExpand || _activeObjects.Count < _config.maxSize || _config.maxSize <= 0)
            {
                obj = UnityEngine.Object.Instantiate(_prefab, position, rotation, parent ?? _parent);
                obj.name = _prefab.name;
            }
            else
            {
                throw new InvalidOperationException($"Pool has reached maximum capacity of {_config.maxSize}");
            }

            obj.SetActive(true);
            _activeObjects.Add(obj);
            
            var poolObjects = obj.GetComponents<IPoolObject>();
            for (int i = 0; i < poolObjects.Length; i++)
            {
                poolObjects[i].OnGetFromPool();
            }
            
            PoolEvents.InvokeObjectGet(_prefab.name, obj);
            
            return obj;
        }

        public T Get<T>() where T : Component
        {
            return Get().GetComponent<T>();
        }

        public T Get<T>(Vector3 position, Quaternion rotation) where T : Component
        {
            return Get(position, rotation).GetComponent<T>();
        }

        public void Release(GameObject obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException(nameof(obj));
            }

            if (!_activeObjects.Remove(obj))
            {
                if (_config.collectionCheck)
                {
                    Debug.LogWarning($"Trying to release GameObject '{obj.name}' that is not from this pool or has already been released");
                }
                return;
            }

            var poolObjects = obj.GetComponents<IPoolObject>();
            for (int i = 0; i < poolObjects.Length; i++)
            {
                poolObjects[i].OnReleaseToPool();
            }
            
            PoolEvents.InvokeObjectRelease(_prefab.name, obj);

            if (_releaseCallbacks.TryGetValue(obj, out var callback))
            {
                callback?.Invoke(obj);
                _releaseCallbacks.Remove(obj);
            }

            if (!_disposed)
            {
                obj.SetActive(false);
                obj.transform.SetParent(_parent);
                
                if (_config.maxSize <= 0 || _pool.Count < _config.maxSize)
                {
                    _pool.Push(obj);
                }
                else
                {
                    DestroyObject(obj);
                }
            }
            else
            {
                DestroyObject(obj);
            }
        }

        public void ReleaseAll()
        {
            for (int i = _activeObjects.Count - 1; i >= 0; i--)
            {
                var obj = _activeObjects[i];
                if (obj != null)
                {
                    Release(obj);
                }
            }
            _activeObjects.Clear();
        }

        public void SetReleaseCallback(GameObject obj, Action<GameObject> callback)
        {
            if (obj != null && _activeObjects.Contains(obj))
            {
                _releaseCallbacks[obj] = callback;
            }
        }

        public void Clear()
        {
            foreach (var obj in _pool)
            {
                if (obj != null)
                {
                    DestroyObject(obj);
                }
            }
            
            _pool.Clear();
            _activeObjects.Clear();
            _releaseCallbacks.Clear();
        }

        public void Preload(int count)
        {
            for (int i = 0; i < count; i++)
            {
                if (_config.maxSize > 0 && _pool.Count >= _config.maxSize)
                    break;
                    
                var obj = UnityEngine.Object.Instantiate(_prefab, _parent);
                obj.name = _prefab.name;
                obj.SetActive(false);
                
                var poolObjects = obj.GetComponents<IPoolObject>();
                for (int j = 0; j < poolObjects.Length; j++)
                {
                    poolObjects[j].OnReleaseToPool();
                }
                
                _pool.Push(obj);
            }
        }

        private void DestroyObject(GameObject obj)
        {
            if (obj == null)
                return;
            
            var poolObjects = obj.GetComponents<IPoolObject>();
            for (int i = 0; i < poolObjects.Length; i++)
            {
                poolObjects[i].OnDestroyFromPool();
            }
            
            UnityEngine.Object.Destroy(obj);
        }

        public void Dispose()
        {
            if (_disposed)
                return;
            
            _disposed = true;
            Clear();
        }

        public override string ToString()
        {
            return $"GameObjectPool[{_prefab?.name}]: Count={Count}, ActiveCount={ActiveCount}, Capacity={Capacity}";
        }
    }
}
