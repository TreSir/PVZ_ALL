using System;

namespace ObjectPool
{
    [Serializable]
    public class PoolConfig
    {
        [UnityEngine.Tooltip("初始池大小")]
        public int initialSize = 10;
        
        [UnityEngine.Tooltip("最大池容量，0表示无限制")]
        public int maxSize = 100;
        
        [UnityEngine.Tooltip("是否自动扩容")]
        public bool autoExpand = true;
        
        [UnityEngine.Tooltip("场景切换时是否自动释放")]
        public bool autoReleaseOnSceneChange = true;
        
        [UnityEngine.Tooltip("自动释放间隔(秒)，-1表示禁用")]
        public float autoReleaseInterval = -1f;
        
        [UnityEngine.Tooltip("是否检查重复释放")]
        public bool collectionCheck = true;

        public PoolConfig() { }

        public PoolConfig(int initialSize, int maxSize = 100, bool autoExpand = true)
        {
            this.initialSize = initialSize;
            this.maxSize = maxSize;
            this.autoExpand = autoExpand;
        }

        public static PoolConfig Default => new PoolConfig();
        
        public static PoolConfig Small => new PoolConfig(5, 20);
        
        public static PoolConfig Medium => new PoolConfig(20, 100);
        
        public static PoolConfig Large => new PoolConfig(50, 500);
        
        public static PoolConfig Unlimited => new PoolConfig(10, 0, true);
    }
}
