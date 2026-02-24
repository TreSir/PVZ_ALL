using System;

namespace ObjectPool
{
    public interface IPool<T> where T : class
    {
        int Count { get; }
        int ActiveCount { get; }
        int Capacity { get; set; }
        
        T Get();
        void Release(T obj);
        void Clear();
        void Preload(int count);
    }
}
