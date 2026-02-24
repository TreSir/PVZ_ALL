using System;
using System.Collections.Generic;

namespace ObjectPool
{
    public class ObjectPool<T> : IPool<T>, IDisposable where T : class
    {
        private readonly Stack<T> _pool;
        private readonly Func<T> _createFunc;
        private readonly Action<T> _actionOnGet;
        private readonly Action<T> _actionOnRelease;
        private readonly Action<T> _actionOnDestroy;
        private readonly PoolConfig _config;
        private readonly HashSet<T> _activeObjects;
        private int _activeCount;
        private bool _disposed;

        public int Count => _pool.Count;
        
        public int ActiveCount => _activeCount;
        
        public int Capacity
        {
            get => _config.maxSize;
            set => _config.maxSize = value;
        }

        public ObjectPool(
            Func<T> createFunc = null,
            Action<T> actionOnGet = null,
            Action<T> actionOnRelease = null,
            Action<T> actionOnDestroy = null,
            PoolConfig config = null)
        {
            _config = config ?? PoolConfig.Default;
            _pool = new Stack<T>(_config.initialSize);
            _createFunc = createFunc ?? (() => (T)Activator.CreateInstance(typeof(T)));
            _actionOnGet = actionOnGet;
            _actionOnRelease = actionOnRelease;
            _actionOnDestroy = actionOnDestroy;
            _activeObjects = _config.collectionCheck ? new HashSet<T>() : null;
            _activeCount = 0;
        }

        public T Get()
        {
            T obj;
            
            if (_pool.Count > 0)
            {
                obj = _pool.Pop();
            }
            else if (_config.autoExpand || _activeCount < _config.maxSize || _config.maxSize <= 0)
            {
                obj = _createFunc();
            }
            else
            {
                throw new InvalidOperationException($"Pool has reached maximum capacity of {_config.maxSize}");
            }

            _activeCount++;
            _activeObjects?.Add(obj);
            
            if (obj is IPoolObject poolObject)
            {
                poolObject.OnGetFromPool();
            }
            
            _actionOnGet?.Invoke(obj);
            
            return obj;
        }

        public PoolHandle<T> GetHandle()
        {
            return new PoolHandle<T>(Get(), this);
        }

        public void Release(T obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException(nameof(obj));
            }

            if (_activeObjects != null)
            {
                if (!_activeObjects.Remove(obj))
                {
                    throw new InvalidOperationException("Trying to release an object that is not from this pool or has already been released");
                }
            }

            _activeCount--;

            if (obj is IPoolObject poolObject)
            {
                poolObject.OnReleaseToPool();
            }
            
            _actionOnRelease?.Invoke(obj);

            if (!_disposed)
            {
                if (_config.maxSize <= 0 || _pool.Count < _config.maxSize)
                {
                    _pool.Push(obj);
                }
                else
                {
                    _actionOnDestroy?.Invoke(obj);
                    
                    if (obj is IPoolObject poolObj)
                    {
                        poolObj.OnDestroyFromPool();
                    }
                }
            }
        }

        public void Clear()
        {
            while (_pool.Count > 0)
            {
                var obj = _pool.Pop();
                _actionOnDestroy?.Invoke(obj);
                
                if (obj is IPoolObject poolObject)
                {
                    poolObject.OnDestroyFromPool();
                }
            }
            
            _activeCount = 0;
            _activeObjects?.Clear();
        }

        public void Preload(int count)
        {
            for (int i = 0; i < count; i++)
            {
                if (_config.maxSize > 0 && _pool.Count >= _config.maxSize)
                    break;
                    
                var obj = _createFunc();
                
                if (obj is IPoolObject poolObject)
                {
                    poolObject.OnReleaseToPool();
                }
                
                _actionOnRelease?.Invoke(obj);
                _pool.Push(obj);
            }
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
            return $"ObjectPool<{typeof(T).Name}>: Count={Count}, ActiveCount={ActiveCount}, Capacity={Capacity}";
        }
    }
}
