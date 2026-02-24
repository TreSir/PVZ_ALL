using System;
using UnityEngine;

namespace ObjectPool
{
    public static class PoolEvents
    {
        public static Action<string, int> OnPoolCreated;
        public static Action<string> OnPoolRemoved;
        public static Action OnAllPoolsCleared;
        
        public static Action<string, GameObject> OnObjectGet;
        public static Action<string, GameObject> OnObjectRelease;
        
        public static void InvokePoolCreated(string key, int count)
        {
            OnPoolCreated?.Invoke(key, count);
        }
        
        public static void InvokePoolRemoved(string key)
        {
            OnPoolRemoved?.Invoke(key);
        }
        
        public static void InvokeAllPoolsCleared()
        {
            OnAllPoolsCleared?.Invoke();
        }
        
        public static void InvokeObjectGet(string key, GameObject obj)
        {
            OnObjectGet?.Invoke(key, obj);
        }
        
        public static void InvokeObjectRelease(string key, GameObject obj)
        {
            OnObjectRelease?.Invoke(key, obj);
        }
    }
}
