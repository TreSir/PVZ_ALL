using System;
using System.Collections;
using UnityEngine;

namespace ObjectPool
{
    public static class PoolExtensions
    {
        public static GameObject GetFromPool(this GameObject prefab, string key = null, Vector3 position = default, Quaternion rotation = default)
        {
            key = key ?? prefab.name;
            return ObjectPoolManager.Instance.Spawn(key, prefab, position, rotation);
        }

        public static T GetFromPool<T>(this GameObject prefab, string key = null, Vector3 position = default, Quaternion rotation = default) where T : Component
        {
            var obj = GetFromPool(prefab, key, position, rotation);
            return obj != null ? obj.GetComponent<T>() : null;
        }

        public static void ReleaseToPool(this GameObject obj, string key = null)
        {
            if (obj == null)
                return;
            
            key = key ?? obj.name;
            
            if (ObjectPoolManager.HasInstance)
            {
                ObjectPoolManager.Instance.Despawn(key, obj);
            }
            else
            {
                Debug.LogWarning($"[PoolExtensions] ObjectPoolManager not available, destroying object '{obj.name}'");
                UnityEngine.Object.Destroy(obj);
            }
        }

        public static void ReleaseToPoolDelayed(this GameObject obj, float delay, string key = null)
        {
            if (obj == null)
                return;
            
            var runner = CoroutineRunner.Instance;
            if (runner != null)
            {
                runner.StartCoroutine(ReleaseDelayedCoroutine(obj, delay, key));
            }
        }

        private static IEnumerator ReleaseDelayedCoroutine(GameObject obj, float delay, string key)
        {
            yield return new WaitForSeconds(delay);
            
            if (obj != null)
            {
                obj.ReleaseToPool(key);
            }
        }

        public static GameObjectPool CreatePool(this GameObject prefab, string key = null, PoolConfig config = null)
        {
            key = key ?? prefab.name;
            return ObjectPoolManager.Instance.GetOrCreatePool(key, prefab, config);
        }

        public static GameObjectPool CreatePool(this GameObject prefab, int initialSize, int maxSize = 100, string key = null)
        {
            key = key ?? prefab.name;
            var config = new PoolConfig(initialSize, maxSize);
            return ObjectPoolManager.Instance.GetOrCreatePool(key, prefab, config);
        }
    }

    internal class CoroutineRunner : MonoBehaviour
    {
        private static CoroutineRunner _instance;
        
        public static CoroutineRunner Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject("[CoroutineRunner]");
                    _instance = go.AddComponent<CoroutineRunner>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }
    }

    public static class PoolHelper
    {
        public static void ReleaseAfterSeconds(GameObject obj, float seconds, string key = null)
        {
            obj.ReleaseToPoolDelayed(seconds, key);
        }

        public static void ReleaseAfterFrames(GameObject obj, int frames, string key = null)
        {
            if (obj == null)
                return;
            
            CoroutineRunner.Instance.StartCoroutine(ReleaseAfterFramesCoroutine(obj, frames, key));
        }

        private static IEnumerator ReleaseAfterFramesCoroutine(GameObject obj, int frames, string key)
        {
            for (int i = 0; i < frames; i++)
            {
                yield return null;
            }
            
            if (obj != null)
            {
                obj.ReleaseToPool(key);
            }
        }
    }
}
