using ObjectPool;

// 获取对象
var bullet = bulletPrefab.GetFromPool(position, rotation);

// 释放对象
bullet.ReleaseToPool();

// 延迟释放
bullet.ReleaseToPoolDelayed(2f);using ObjectPool;

// 获取对象
var bullet = bulletPrefab.GetFromPool(position, rotation);

// 释放对象
bullet.ReleaseToPool();

// 延迟释放
bullet.ReleaseToPoolDelayed(2f);using ObjectPool;

// 获取对象
var bullet = bulletPrefab.GetFromPool(position, rotation);

// 释放对象
bullet.ReleaseToPool();

// 延迟释放
bullet.ReleaseToPoolDelayed(2f);# Unity 对象池系统 - 使用教程

## 快速开始

### 1. 导入项目

将 `ObjectPool` 文件夹复制到你的 Unity 项目的 `Assets` 目录下即可。

### 2. 基础概念

本系统提供三种类型的对象池：

| 池类型 | 用途 | 特点 |
|--------|------|------|
| `GameObjectPool` | GameObject 对象池 | 用于子弹、特效、敌人等频繁创建销毁的 GameObject |
| `ObjectPool<T>` | 通用 C# 对象池 | 用于普通 C# 类的复用 |
| `ListPool<T>` / `DictionaryPool<K,V>` | 集合池 | 用于临时集合，避免 GC |

---

## 一、GameObject 对象池

### 1.1 最简单的使用方式

```csharp
using ObjectPool;
using UnityEngine;

public class SimpleExample : MonoBehaviour
{
    [SerializeField] private GameObject bulletPrefab;
    
    void Start()
    {
        // 方式一：使用扩展方法（推荐）
        var bullet = bulletPrefab.GetFromPool();
        
        // 方式二：使用管理器
        var bullet2 = ObjectPoolManager.Instance.Spawn("Bullets", bulletPrefab);
    }
    
    void Update()
    {
        // 释放对象回池
        bullet.ReleaseToPool();
    }
}
```

### 1.2 创建配置好的池

```csharp
using ObjectPool;
using UnityEngine;

public class ConfiguredPoolExample : MonoBehaviour
{
    [SerializeField] private GameObject enemyPrefab;
    private GameObjectPool _enemyPool;
    
    void Start()
    {
        // 创建配置
        var config = new PoolConfig
        {
            initialSize = 20,       // 初始预创建20个
            maxSize = 100,          // 最大容量100
            autoExpand = true,      // 自动扩容
            collectionCheck = true  // 检查重复释放
        };
        
        // 创建池
        _enemyPool = ObjectPoolManager.Instance.GetOrCreatePool("Enemies", enemyPrefab, config);
    }
    
    public GameObject SpawnEnemy(Vector3 position)
    {
        // 从池中获取
        return _enemyPool.Get(position, Quaternion.identity);
    }
    
    public void DespawnEnemy(GameObject enemy)
    {
        // 释放回池
        _enemyPool.Release(enemy);
    }
}
```

### 1.3 实现池对象接口（推荐）

实现 `IPoolObject` 接口可以自动处理对象的初始化和重置：

```csharp
using ObjectPool;
using UnityEngine;

public class Bullet : MonoBehaviour, IPoolObject
{
    private Rigidbody _rb;
    private TrailRenderer _trail;
    
    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _trail = GetComponent<TrailRenderer>();
    }
    
    // 从池中获取时调用
    public void OnGetFromPool()
    {
        gameObject.SetActive(true);
        _rb.velocity = Vector3.zero;
        _trail.Clear();
    }
    
    // 释放回池时调用
    public void OnReleaseToPool()
    {
        gameObject.SetActive(false);
        _rb.velocity = Vector3.zero;
        transform.position = Vector3.zero;
        transform.rotation = Quaternion.identity;
    }
    
    // 池销毁时调用
    public void OnDestroyFromPool()
    {
        if (gameObject != null)
            Destroy(gameObject);
    }
}
```

### 1.4 延迟释放

```csharp
using ObjectPool;
using UnityEngine;

public class EffectExample : MonoBehaviour
{
    [SerializeField] private GameObject explosionPrefab;
    
    public void CreateExplosion(Vector3 position)
    {
        var effect = explosionPrefab.GetFromPool(position: position);
        
        // 2秒后自动释放
        effect.ReleaseToPoolDelayed(2f);
    }
}
```

---

## 二、通用对象池 (ObjectPool<T>)

### 2.1 基础使用

```csharp
using ObjectPool;
using UnityEngine;

// 可池化的数据类
public class EnemyData
{
    public int health;
    public float speed;
    public Vector3 position;
    
    public void Reset()
    {
        health = 100;
        speed = 1f;
        position = Vector3.zero;
    }
}

public class DataPoolExample : MonoBehaviour
{
    private ObjectPool<EnemyData> _dataPool;
    
    void Start()
    {
        // 创建对象池
        _dataPool = ObjectPoolManager.Instance.GetOrCreatePool<EnemyData>(
            "EnemyData",
            createFunc: () => new EnemyData(),
            actionOnRelease: data => data.Reset()
        );
    }
    
    public EnemyData GetData()
    {
        return _dataPool.Get();
    }
    
    public void ReturnData(EnemyData data)
    {
        _dataPool.Release(data);
    }
}
```

### 2.2 使用句柄自动释放

```csharp
using ObjectPool;

public class HandleExample : MonoBehaviour
{
    public void ProcessEnemy()
    {
        // 使用 using 语句，自动释放
        using (var handle = _dataPool.GetHandle())
        {
            var data = handle.Value;
            data.health = 50;
            data.speed = 2f;
            
            DoSomething(data);
        } // 离开作用域自动释放回池
    }
}
```

---

## 三、集合池 (ListPool / DictionaryPool)

### 3.1 ListPool 使用

```csharp
using ObjectPool;
using System.Collections.Generic;
using UnityEngine;

public class ListPoolExample : MonoBehaviour
{
    public void FindEnemiesInRange(Vector3 center, float range)
    {
        // 方式一：手动获取和释放
        var enemies = ListPool<Enemy>.Get();
        
        try
        {
            // 使用临时列表
            var colliders = Physics.OverlapSphere(center, range);
            foreach (var col in colliders)
            {
                var enemy = col.GetComponent<Enemy>();
                if (enemy != null)
                    enemies.Add(enemy);
            }
            
            ProcessEnemies(enemies);
        }
        finally
        {
            // 释放回池
            ListPool<Enemy>.Release(enemies);
        }
    }
    
    public void FindEnemiesWithHandle()
    {
        // 方式二：使用句柄自动释放（推荐）
        using (var handle = ListPool<Enemy>.GetHandle())
        {
            var enemies = handle.Value;
            
            var colliders = Physics.OverlapSphere(center, range);
            foreach (var col in colliders)
            {
                var enemy = col.GetComponent<Enemy>();
                if (enemy != null)
                    enemies.Add(enemy);
            }
            
            ProcessEnemies(enemies);
        } // 自动释放
    }
}
```

### 3.2 DictionaryPool 使用

```csharp
using ObjectPool;
using System.Collections.Generic;

public class DictPoolExample
{
    public void GroupEnemiesByType(List<Enemy> enemies)
    {
        using (var handle = DictionaryPool<EnemyType, List<Enemy>>.GetHandle())
        {
            var groups = handle.Value;
            
            foreach (var enemy in enemies)
            {
                if (!groups.TryGetValue(enemy.Type, out var list))
                {
                    list = ListPool<Enemy>.Get();
                    groups[enemy.Type] = list;
                }
                list.Add(enemy);
            }
            
            ProcessGroups(groups);
            
            // 清理嵌套的 List
            foreach (var list in groups.Values)
            {
                ListPool<Enemy>.Release(list);
            }
        }
    }
}
```

---

## 四、PoolConfig 配置详解

```csharp
using ObjectPool;

// 创建配置
var config = new PoolConfig
{
    initialSize = 10,               // 初始大小：预创建的对象数量
    maxSize = 100,                  // 最大容量：0表示无限制
    autoExpand = true,              // 自动扩容：池空时是否自动创建
    autoReleaseOnSceneChange = true,// 场景切换时自动释放
    autoReleaseInterval = -1f,      // 自动释放间隔（秒），-1禁用
    collectionCheck = true          // 重复释放检查：防止同一对象被多次释放
};

// 使用预设配置
var smallConfig = PoolConfig.Small;      // 5初始，20最大
var mediumConfig = PoolConfig.Medium;    // 20初始，100最大
var largeConfig = PoolConfig.Large;      // 50初始，500最大
var unlimitedConfig = PoolConfig.Unlimited; // 10初始，无限制
```

---

## 五、扩展方法速查

| 方法 | 说明 | 示例 |
|------|------|------|
| `GetFromPool()` | 从池获取 GameObject | `prefab.GetFromPool()` |
| `GetFromPool(position, rotation)` | 从池获取并设置位置旋转 | `prefab.GetFromPool(pos, rot)` |
| `ReleaseToPool()` | 释放回池 | `obj.ReleaseToPool()` |
| `ReleaseToPoolDelayed(seconds)` | 延迟释放 | `obj.ReleaseToPoolDelayed(2f)` |
| `CreatePool()` | 创建池 | `prefab.CreatePool(20, 100)` |

---

## 六、编辑器调试

### 6.1 打开调试窗口

菜单：`Tools > ObjectPool > 调试窗口`

### 6.2 功能说明

**池列表标签页：**
- 查看所有活动池的状态
- 显示缓存数量、使用中数量、容量
- 预加载对象
- 清空指定池

**性能统计标签页：**
- 获取/释放/创建次数
- 命中率统计
- 总池数量和活跃对象数

---

## 七、最佳实践

### 7.1 预加载

对于已知需要大量使用的对象，在游戏启动时预加载：

```csharp
public class GameInitializer : MonoBehaviour
{
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private GameObject effectPrefab;
    
    IEnumerator Start()
    {
        // 预加载子弹池
        var bulletPool = bulletPrefab.CreatePool("Bullets", PoolConfig.Large);
        yield return null;
        
        // 预加载特效池
        var effectPool = effectPrefab.CreatePool("Effects", PoolConfig.Medium);
        yield return null;
    }
}
```

### 7.2 合理设置容量

```csharp
// 子弹：频繁使用，需要大池
var bulletConfig = new PoolConfig(50, 200);

// Boss：少量使用，小池即可
var bossConfig = new PoolConfig(1, 5);

// 特效：中等使用频率
var effectConfig = PoolConfig.Medium;
```

### 7.3 正确实现 IPoolObject

```csharp
public class GoodExample : MonoBehaviour, IPoolObject
{
    private Rigidbody _rb;
    private Collider _collider;
    private ParticleSystem _ps;
    
    public void OnGetFromPool()
    {
        // 激活对象
        gameObject.SetActive(true);
        
        // 重置物理状态
        if (_rb != null)
        {
            _rb.velocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
        }
        
        // 重置特效
        if (_ps != null)
            _ps.Clear();
    }
    
    public void OnReleaseToPool()
    {
        // 停止所有协程
        StopAllCoroutines();
        
        // 取消所有 Invoke
        CancelInvoke();
        
        // 重置状态
        transform.SetParent(null);
        transform.position = Vector3.zero;
        transform.rotation = Quaternion.identity;
        
        // 停用对象
        gameObject.SetActive(false);
    }
    
    public void OnDestroyFromPool()
    {
        if (gameObject != null)
            Destroy(gameObject);
    }
}
```

### 7.4 避免内存泄漏

```csharp
// 错误：忘记释放
void BadExample()
{
    var bullet = bulletPool.Get();
    // 使用后忘记释放...
}

// 正确：确保释放
void GoodExample()
{
    var bullet = bulletPool.Get();
    StartCoroutine(AutoRelease(bullet, 3f));
}

IEnumerator AutoRelease(GameObject obj, float delay)
{
    yield return new WaitForSeconds(delay);
    if (obj != null)
        obj.ReleaseToPool();
}
```

---

## 八、常见问题

### Q1: 对象池会自动创建吗？

是的，使用 `GetFromPool()` 或 `Spawn()` 时，如果池不存在会自动创建。

### Q2: 如何在场景切换时清理池？

PoolConfig 的 `autoReleaseOnSceneChange` 默认为 true，会自动释放。如需完全清理：

```csharp
ObjectPoolManager.Instance.ClearAllPools();
```

### Q3: 池对象被 Destroy 了怎么办？

池会自动处理，下次获取时会创建新对象。但建议避免手动 Destroy 池对象。

### Q4: 如何监控池的性能？

使用调试窗口（Tools > ObjectPool > 调试窗口）查看命中率和使用统计。

---

## 九、API 速查表

### ObjectPoolManager

| 方法 | 说明 |
|------|------|
| `Instance` | 单例访问 |
| `GetOrCreatePool(key, prefab, config)` | 获取或创建 GameObject 池 |
| `GetOrCreatePool<T>(key, config)` | 获取或创建通用池 |
| `GetPool(key)` | 获取指定池 |
| `HasPool(key)` | 检查池是否存在 |
| `RemovePool(key)` | 移除指定池 |
| `ClearAllPools()` | 清空所有池 |
| `Spawn(key, prefab, position, rotation)` | 快速生成 |
| `Despawn(key, obj)` | 快速释放 |

### GameObjectPool

| 方法 | 说明 |
|------|------|
| `Get()` | 获取对象 |
| `Get(position, rotation)` | 获取并设置位置旋转 |
| `Get<T>()` | 获取并获取组件 |
| `Release(obj)` | 释放对象 |
| `ReleaseAll()` | 释放所有活跃对象 |
| `Preload(count)` | 预加载 |
| `Clear()` | 清空池 |

### ListPool<T> / DictionaryPool<K,V>

| 方法 | 说明 |
|------|------|
| `Get()` | 获取集合 |
| `GetHandle()` | 获取句柄（自动释放） |
| `Release(collection)` | 释放集合 |
| `Clear()` | 清空池 |
| `WarmUp(count)` | 预热 |

---

## 十、文件结构

```
ObjectPool/
├── Scripts/
│   ├── Core/
│   │   ├── IPool.cs              # 池接口
│   │   ├── IPoolObject.cs        # 池对象接口
│   │   ├── PoolConfig.cs         # 池配置
│   │   └── PoolHandle.cs         # 安全句柄
│   ├── Pool/
│   │   ├── ObjectPool.cs         # 通用对象池
│   │   ├── GameObjectPool.cs     # GameObject 池
│   │   ├── ListPool.cs           # List 池
│   │   └── DictionaryPool.cs     # Dictionary 池
│   ├── Manager/
│   │   └── ObjectPoolManager.cs  # 全局管理器
│   └── Extension/
│       ├── PoolExtensions.cs     # 扩展方法
│       └── PoolEvents.cs         # 事件系统
└── Editor/
    ├── ObjectPoolEditor.cs       # Inspector 扩展
    └── PoolDebuggerWindow.cs     # 调试窗口
```
