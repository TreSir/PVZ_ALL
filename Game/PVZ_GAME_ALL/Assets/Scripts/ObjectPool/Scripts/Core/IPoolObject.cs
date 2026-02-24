using UnityEngine;

namespace ObjectPool
{
    public interface IPoolObject
    {
        void OnGetFromPool();
        void OnReleaseToPool();
        void OnDestroyFromPool();
    }

    public interface IGameObjectPoolObject : IPoolObject
    {
        GameObject GameObject { get; }
        Transform Transform { get; }
    }
}
