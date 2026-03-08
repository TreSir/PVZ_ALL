# ObjectPool 对象池模块使用指南

## 概述

ObjectPool 提供高性能对象池系统，管理 GameObject 和普通对象的复用，减少 GC 压力。

---

## 基本使用

### GameObject 对象池

```csharp
using ObjectPool;

GameObject prefab = Resources.Load<GameObject>("Prefabs/Enemy");

// 获取对象
GameObject enemy = ObjectPoolManager.Instance.Spawn("Enemy", prefab, position, rotation);

// 归还对象
ObjectPoolManager.Instance.Despawn("Enemy", enemy);
```

### 泛型对象池

```csharp
var pool = ObjectPoolManager.Instance.GetOrCreatePool<MyData>("MyDataPool");

MyData data = pool.Get();
pool.Release(data);
```

---

## 池配置

```csharp
var config = new PoolConfig
{
    initialSize = 10,           // 初始大小
    maxSize = 100,              // 最大容量
    autoExpand = true,          // 自动扩容
    autoReleaseOnSceneChange = true
};

var pool = ObjectPoolManager.Instance.GetOrCreatePool("Enemy", prefab, config);
```

### 预设配置

```csharp
PoolConfig.Default    // (10, 100)
PoolConfig.Small      // (5, 20)
PoolConfig.Medium     // (20, 100)
PoolConfig.Large      // (50, 500)
PoolConfig.Unlimited  // (10, 0)
```

---

## 自定义池化对象

```csharp
public class PooledEnemy : MonoBehaviour, IGameObjectPoolObject
{
    public GameObject GameObject => gameObject;
    public Transform Transform => transform;

    public void OnGetFromPool() => gameObject.SetActive(true);
    public void OnReleaseToPool() => gameObject.SetActive(false);
    public void OnDestroyFromPool() => Destroy(gameObject);
}
```

---

## API 速查

| 方法 | 说明 |
|------|------|
| `Spawn(key, prefab, pos, rot)` | 获取对象 |
| `Despawn(key, obj)` | 归还对象 |
| `GetOrCreatePool(key, prefab)` | 创建/获取池 |
| `Preload(key, count)` | 预加载 |
| `ClearPool(key)` | 清空池 |
| `ClearAllPools()` | 清空所有池 |

---

## 注意事项

- 场景切换时可自动释放
- 使用编辑器窗口调试：Window > Object Pool Debugger
