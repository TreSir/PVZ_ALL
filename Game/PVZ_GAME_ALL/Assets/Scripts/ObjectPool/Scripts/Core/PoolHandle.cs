using System;

namespace ObjectPool
{
    public struct PoolHandle<T> : IDisposable where T : class
    {
        private T _value;
        private IPool<T> _pool;
        private bool _disposed;

        public T Value => _value;
        
        public bool IsValid => !_disposed && _value != null;
        
        public bool HasValue => _value != null;

        internal PoolHandle(T value, IPool<T> pool)
        {
            _value = value;
            _pool = pool;
            _disposed = false;
        }

        public void Dispose()
        {
            if (_disposed)
                return;
            
            _disposed = true;
            
            if (_pool != null && _value != null)
            {
                _pool.Release(_value);
            }
            
            _value = null;
            _pool = null;
        }

        public static PoolHandle<T> Empty => new PoolHandle<T>(null, null);

        public static implicit operator bool(PoolHandle<T> handle)
        {
            return handle.IsValid;
        }
    }

    public struct ListPoolHandle<T> : IDisposable
    {
        private System.Collections.Generic.List<T> _list;
        private bool _disposed;

        public System.Collections.Generic.List<T> Value => _list;
        
        public bool IsValid => !_disposed && _list != null;

        internal ListPoolHandle(System.Collections.Generic.List<T> list)
        {
            _list = list;
            _disposed = false;
        }

        public void Dispose()
        {
            if (_disposed)
                return;
            
            _disposed = true;
            
            if (_list != null)
            {
                ListPool<T>.Release(_list);
            }
            
            _list = null;
        }

        public static implicit operator bool(ListPoolHandle<T> handle)
        {
            return handle.IsValid;
        }
    }

    public struct DictionaryPoolHandle<TKey, TValue> : IDisposable
    {
        private System.Collections.Generic.Dictionary<TKey, TValue> _dictionary;
        private bool _disposed;

        public System.Collections.Generic.Dictionary<TKey, TValue> Value => _dictionary;
        
        public bool IsValid => !_disposed && _dictionary != null;

        internal DictionaryPoolHandle(System.Collections.Generic.Dictionary<TKey, TValue> dictionary)
        {
            _dictionary = dictionary;
            _disposed = false;
        }

        public void Dispose()
        {
            if (_disposed)
                return;
            
            _disposed = true;
            
            if (_dictionary != null)
            {
                DictionaryPool<TKey, TValue>.Release(_dictionary);
            }
            
            _dictionary = null;
        }

        public static implicit operator bool(DictionaryPoolHandle<TKey, TValue> handle)
        {
            return handle.IsValid;
        }
    }
}
