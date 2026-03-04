# ObjectPool 对象池模块使用指南

## 概述

ObjectPool 模块提供高性能的对象池系统，用于管理游戏中的 GameObject 和普通对象的重复创建与销毁。通过对象池，可以显著减少 GC 压力，提高游戏性能。

## 核心概念

### 对象池管理器 (ObjectPoolManager)

统一管理所有对象池的单例类，负责创建、获取、释放对象池。

### 池配置 (PoolConfig)

```csharp
public class PoolConfig
{
    public int initialSize = 10;           // 初始池大小
    public int maxSize = 100;               // 最大池容量，0表示无限制
    public bool autoExpand = true;          // 是否自动扩容
    public bool autoReleaseOnSceneChange = true; // 场景切换时自动释放
    public float autoReleaseInterval = -1f;    // 自动释放间隔
    public bool collectionCheck = true;    // 是否检查重复释放
}
```

### 预设配置

```csharp
PoolConfig.Default    // 默认配置 (10, 100)
PoolConfig.Small      // 小型池 (5, 20)
PoolConfig.Medium     // 中型池 (20, 100)
PoolConfig.Large      // 大型池 (50, 500)
PoolConfig.Unlimited  // 无限制池 (10, 0)
```

## 基本使用

### 获取对象池管理器

```csharp
// 通过单例获取
ObjectPoolManager poolManager = ObjectPoolManager.Instance;

// 检查是否有效
if (ObjectPoolManager.HasInstance)
{
    // 使用对象池
}
```

### GameObject 对象池

#### 方式一：通过管理器直接使用

```csharp
// 1. 创建或获取对象池
GameObject prefab = Resources.Load<GameObject>("Prefabs/Enemy");
var pool = ObjectPoolManager.Instance.GetOrCreatePool("Enemy", prefab);

// 2. 从池中获取对象
GameObject enemy = pool.Get();

// 设置位置和旋转
enemy.transform.position = new Vector3(0, 0, 0);
enemy.transform.rotation = Quaternion.identity;

// 3. 使用完毕后归还池
pool.Release(enemy);
```

#### 方式二：使用快捷方法

```csharp
// 获取对象（如果池不存在会自动创建）
GameObject enemy = ObjectPoolManager.Instance.Spawn("Enemy", prefab, position, rotation);

// 归还对象
ObjectPoolManager.Instance.Despawn("Enemy", enemy);
```

### 泛型对象池

用于非 GameObject 对象的池化，如数据结构、配置类等：

```csharp
// 创建泛型对象池
var pool = ObjectPoolManager.Instance.GetOrCreatePool<MyClass>("MyClassPool");

// 获取对象
MyClass obj = pool.Get();

// 使用完毕后归还
pool.Release(obj);

// 或者使用便捷方法
ObjectPoolManager.Instance.GetOrCreatePool<MyData>("MyDataPool",
    createFunc: () => new MyData(),  // 自定义创建函数
    actionOnGet: obj => obj.Reset(), // 获取时重置
    actionOnRelease: obj => obj.Clear() // 归还时清理
);
```

## 实现自定义池化对象

### 实现 IPoolObject 接口

```csharp
using UnityEngine;
using ObjectPool;

public class PooledEnemy : MonoBehaviour, IGameObjectPoolObject
{
    private EnemyAI _ai;

    public GameObject GameObject => gameObject;
    public Transform Transform => transform;

    private void Awake()
    {
        _ai = GetComponent<EnemyAI>();
    }

    // 从池中取出时调用
    public void OnGetFromPool()
    {
        gameObject.SetActive(true);
        _ai.Reset();
    }

    // 归还到池时调用
    public void OnReleaseToPool()
    {
        gameObject.SetActive(false);
    }

    // 从池中销毁时调用
    public void OnDestroyFromPool()
    {
        Destroy(gameObject);
    }

    // 初始化方法（可选）
    public void Initialize(EnemyData data)
    {
        _ai.SetData(data);
    }
}
```

### 使用自定义池化对象

```csharp
// 创建池时传入 Prefab
GameObject prefab = Resources.Load<GameObject>("Prefabs/Enemy");
var pool = ObjectPoolManager.Instance.GetOrCreatePool("Enemy", prefab);

// 获取对象
GameObject enemyObj = pool.Get();
var pooledEnemy = enemyObj.GetComponent<PooledEnemy>();
pooledEnemy.Initialize(enemyData);

// 归还对象
pool.Release(enemyObj);
```

## 配置详解

### 池配置选项

```csharp
// 创建自定义配置
var config = new PoolConfig
{
    initialSize = 5,           // 初始创建 5 个对象
    maxSize = 20,              // 最多 20 个对象
    autoExpand = true,         // 池满时自动扩容
    autoReleaseOnSceneChange = true, // 切换场景时释放
    autoReleaseInterval = 30f, // 30秒后自动释放空闲对象
    collectionCheck = true      // 启用重复释放检查
};

var pool = ObjectPoolManager.Instance.GetOrCreatePool("Enemy", prefab, config);
```

### 预加载对象

```csharp
// 预先创建指定数量的对象
pool.Preload(10);

// 或者通过管理器
ObjectPoolManager.Instance.Preload("Enemy", 10);
```

## 池管理操作

### 查询池信息

```csharp
// 检查是否存在指定池
if (ObjectPoolManager.Instance.HasPool("Enemy"))
{
    // 池存在
}

// 获取所有池的信息
var poolInfos = ObjectPoolManager.Instance.GetAllPoolInfo();
foreach (var info in poolInfos)
{
    Debug.Log($"Pool: {info.Key}, Count: {info.Count}, Active: {info.ActiveCount}");
}

// 获取池的引用
var pool = ObjectPoolManager.Instance.GetPool("Enemy");
```

### 删除池

```csharp
// 删除指定池
ObjectPoolManager.Instance.RemovePool("Enemy");

// 删除所有池
ObjectPoolManager.Instance.ClearAllPools();
```

## 实用示例

### 示例1：子弹池

```csharp
public class BulletPoolExample : MonoBehaviour
{
    private GameObjectPool _bulletPool;
    public GameObject bulletPrefab;
    public int initialSize = 50;

    private void Start()
    {
        // 创建子弹池
        _bulletPool = ObjectPoolManager.Instance.GetOrCreatePool(
            "Bullet",
            bulletPrefab,
            new PoolConfig(initialSize, 200)
        );
    }

    public GameObject SpawnBullet(Vector3 position, Vector3 direction)
    {
        var bullet = _bulletPool.Get(position, Quaternion.LookRotation(direction));
        return bullet;
    }

    public void DespawnBullet(GameObject bullet)
    {
        _bulletPool.Release(bullet);
    }
}
```

### 示例2：粒子特效池

```csharp
public class VFXPoolExample : MonoBehaviour
{
    private GameObjectPool _vfxPool;

    private void Start()
    {
        var prefab = Resources.Load<GameObject>("VFX/Explosion");
        _vfxPool = ObjectPoolManager.Instance.GetOrCreatePool("VFX", prefab,
            new PoolConfig(5, 30));
    }

    public void PlayVFX(Vector3 position)
    {
        var vfx = _vfxPool.Get(position, Quaternion.identity);
        var particle = vfx.GetComponent<ParticleSystem>();

        // 播放完毕后自动归还
        StartCoroutine(ReturnAfterPlay(vfx, particle.main.duration));
    }

    private IEnumerator ReturnAfterPlay(GameObject vfx, float duration)
    {
        yield return new WaitForSeconds(duration);
        _vfxPool.Release(vfx);
    }
}
```

### 示例3：数据对象池

```csharp
public class DamageDataPoolExample : MonoBehaviour
{
    private ObjectPool<DamageData> _damagePool;

    private void Start()
    {
        _damagePool = ObjectPoolManager.Instance.GetOrCreatePool<DamageData>(
            "DamageData",
            createFunc: () => new DamageData(),
            actionOnGet: d => d.Reset(),
            actionOnRelease: d => d.Clear(),
            config: PoolConfig.Small
        );
    }

    public void ProcessDamage(int targetID, int damage)
    {
        // 从池中获取数据
        var damageData = _damagePool.Get();
        damageData.TargetID = targetID;
        damageData.Damage = damage;
        damageData.Timestamp = Time.time;

        // 处理伤害
        ApplyDamage(damageData);

        // 归还到池
        _damagePool.Release(damageData);
    }
}

public class DamageData
{
    public int TargetID;
    public int Damage;
    public float Timestamp;

    public void Reset() { }

    public void Clear()
    {
        TargetID = 0;
        Damage = 0;
        Timestamp = 0;
    }
}
```

### 示例4：3D 对象快速生成

```csharp
public class SpawnerExample : MonoBehaviour
{
    public GameObject enemyPrefab;
    public int waveSize = 10;

    private void Start()
    {
        // 创建怪物池
        ObjectPoolManager.Instance.GetOrCreatePool(
            "Enemy",
            enemyPrefab,
            new PoolConfig(waveSize, waveSize * 3)
        );
    }

    private void SpawnWave()
    {
        for (int i = 0; i < waveSize; i++)
        {
            Vector3 randomPos = Random.insideUnitSphere * 10f;
            var enemy = ObjectPoolManager.Instance.Spawn("Enemy", enemyPrefab, randomPos, Quaternion.identity);

            // 设置敌人生成数据
            var enemyAI = enemy.GetComponent<EnemyAI>();
            enemyAI.Initialize(Random.Range(1, 10));
        }
    }

    private void回收所有敌人()
    {
        var pool = ObjectPoolManager.Instance.GetPool("Enemy");
        if (pool != null)
        {
            pool.ReleaseAll();
        }
    }
}
```

## 与其他模块集成

### 与 AudioManager 集成

AudioManager 已经集成了对象池，用于管理 AudioSource：

```csharp
// AudioManager 内部使用对象池
// 配置文件在 Resources/Configs/GameSettingConfig.asset
// Pool Settings:
// - Audio Initial Pool Size: 10
// - Audio Max Pool Size: 30
```

### 与 UI 模块集成

UI 模块也实现了面板对象池：

```csharp
// 启用 UI 面板池化
BaseUIManager.Instance.SetUsePool(true, 3);

// 预加载面板
BaseUIManager.Instance.PreloadPanel("MainMenuPanel");
```

## 性能优化建议

### 1. 合理设置池大小

```csharp
// ❌ 不好：过小的初始池
var pool = ObjectPoolManager.Instance.GetOrCreatePool("Enemy", prefab,
    new PoolConfig(1, 10)); // 频繁创建销毁

// ✅ 好：根据实际需求设置
var pool = ObjectPoolManager.Instance.GetOrCreatePool("Enemy", prefab,
    new PoolConfig(20, 100)); // 合理的初始大小
```

### 2. 预加载常用对象

```csharp
private void Start()
{
    // 游戏开始时预加载
    var pool = ObjectPoolManager.Instance.GetOrCreatePool("Bullet", bulletPrefab);
    pool.Preload(100); // 预创建 100 发子弹
}
```

### 3. 使用回调函数自动重置

```csharp
var pool = ObjectPoolManager.Instance.GetOrCreatePool<MyObject>(
    "MyObject",
    createFunc: () => new MyObject(),
    actionOnGet: obj => obj.OnGet(),  // 自动调用
    actionOnRelease: obj => obj.OnRelease()
);
```

### 4. 场景切换时清理

```csharp
// 自动清理（默认开启）
// 切换场景时自动释放所有池
// 如需手动控制：
var config = new PoolConfig
{
    autoReleaseOnSceneChange = false // 禁用自动释放
};
```

## 注意事项

1. **Prefab 依赖**：GameObject 池需要预先加载 Prefab
2. **对象状态**：归还池的对象会被 SetActive(false)，取出时设为 true
3. **重复释放**：启用 collectionCheck 后，重复释放会抛出异常
4. **内存泄漏**：确保所有借出的对象都被归还
5. **线程安全**：对象池操作是线程安全的，但建议在主线程使用

## 相关文件

- [ObjectPoolManager.cs](../ObjectPool/Scripts/Manager/ObjectPoolManager.cs) - 池管理器
- [GameObjectPool.cs](../ObjectPool/Scripts/Pool/GameObjectPool.cs) - GameObject 池
- [ObjectPool.cs](../ObjectPool/Scripts/Pool/ObjectPool.cs) - 泛型池
- [PoolConfig.cs](../ObjectPool/Scripts/Core/PoolConfig.cs) - 池配置
- [IPoolObject.cs](../ObjectPool/Scripts/Core/IPoolObject.cs) - 池对象接口
