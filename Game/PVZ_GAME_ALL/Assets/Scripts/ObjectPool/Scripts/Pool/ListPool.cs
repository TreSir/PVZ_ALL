using System;
using System.Collections.Generic;

namespace ObjectPool
{
    public static class ListPool<T>
    {
        private static readonly Stack<List<T>> _pool = new Stack<List<T>>();
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

        public static List<T> Get()
        {
            List<T> list;
            
            lock (_lock)
            {
                if (_pool.Count > 0)
                {
                    list = _pool.Pop();
                }
                else
                {
                    list = new List<T>();
                }
            }
            
            _activeCount++;
            return list;
        }

        public static ListPoolHandle<T> GetHandle()
        {
            return new ListPoolHandle<T>(Get());
        }

        public static void Release(List<T> list)
        {
            if (list == null)
            {
                throw new ArgumentNullException(nameof(list));
            }

            list.Clear();
            _activeCount--;
            
            lock (_lock)
            {
                _pool.Push(list);
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
                    _pool.Push(new List<T>());
                }
            }
        }
    }
}
