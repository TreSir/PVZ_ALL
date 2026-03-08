# Core 核心模块使用指南

## 概述

Core 模块是游戏的**初始化中心**，通过反射自动发现并初始化所有 `IGameSystem` 实现。

---

## 系统接口

```csharp
public interface IGameSystem
{
    int Priority { get; }    // 初始化优先级，数字越小越先初始化
    void Initialize();       // 初始化
    void Shutdown();         // 关闭
}

public interface IGameSystemPreload : IGameSystem { void Preload(); }
public interface IGameSystemUpdate : IGameSystem { void Update(float deltaTime); }
public interface IGameSystemLateUpdate : IGameSystem { void LateUpdate(float deltaTime); }
```

---

## 使用示例

### 创建游戏系统

```csharp
using GameBase;

public class MySystem : IGameSystem
{
    public int Priority => 100;  // 高优先级先初始化

    public void Initialize() { }
    public void Shutdown() { }
}
```

### 创建带帧更新的系统

```csharp
public class MyUpdateSystem : IGameSystem, IGameSystemUpdate
{
    public int Priority => 300;

    public void Initialize() { }
    public void Update(float deltaTime) { }
    public void Shutdown() { }
}
```

---

## 初始化流程

```
GameInitializer.Awake()
       │
       ├─► DiscoverGameSystems()  扫描所有程序集
       │
       ├─► RegisterSystem()       注册系统
       │
       └─► InitializeAllSystems() 按 Priority 初始化
```

---

## 优先级建议

| 范围 | 用途 |
|------|------|
| 0-100 | 核心系统（数据、配置） |
| 100-500 | 基础系统（资源、对象池） |
| 500-1000 | 业务系统 |
| 1000+ | 功能系统 |

---

## 当前系统

| 系统 | Priority |
|------|----------|
| DataManager | 50 |
| SaveManager | 51 |
| AudioSystem | 1000 |
| BaseUIManager | 2000 |

---

## 注意事项

- 类必须有公开无参构造函数
- 不要手动调用 Initialize/Shutdown
- 单例类需在构造函数中设置 `_instance = this`
