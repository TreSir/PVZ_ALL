using System;
using System.Collections.Generic;

namespace ObjectPool
{
    public static class DictionaryPool<TKey, TValue>
    {
        private static readonly Stack<Dictionary<TKey, TValue>> _pool = new Stack<Dictionary<TKey, TValue>>();
        private static readonly object _lock = new object();
        private static int _activeCount;

        public static int Count
        {
            get
            {
                lock (_lock)
                {
                    return _pool.Count;
                }
            }
        }

        public static int ActiveCount => _activeCount;

        public static Dictionary<TKey, TValue> Get()
        {
            Dictionary<TKey, TValue> dict;
            
            lock (_lock)
            {
                if (_pool.Count > 0)
                {
                    dict = _pool.Pop();
                }
                else
                {
                    dict = new Dictionary<TKey, TValue>();
                }
            }
            
            _activeCount++;
            return dict;
        }

        public static Dictionary<TKey, TValue> Get(int capacity)
        {
            Dictionary<TKey, TValue> dict;
            
            lock (_lock)
            {
                if (_pool.Count > 0)
                {
                    dict = _pool.Pop();
                }
                else
                {
                    dict = new Dictionary<TKey, TValue>(capacity);
                }
            }
            
            _activeCount++;
            return dict;
        }

        public static DictionaryPoolHandle<TKey, TValue> GetHandle()
        {
            return new DictionaryPoolHandle<TKey, TValue>(Get());
        }

        public static void Release(Dictionary<TKey, TValue> dict)
        {
            if (dict == null)
            {
                throw new ArgumentNullException(nameof(dict));
            }

            dict.Clear();
            _activeCount--;
            
            lock (_lock)
            {
                _pool.Push(dict);
            }
        }

        public static void Clear()
        {
            lock (_lock)
            {
                _pool.Clear();
            }
        }

        public static void WarmUp(int count)
        {
            lock (_lock)
            {
                for (int i = 0; i < count; i++)
                {
                    _pool.Push(new Dictionary<TKey, TValue>());
                }
            }
        }
    }
}
