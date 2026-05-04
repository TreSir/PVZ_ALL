# Core 核心模块使用指南

## 概述

Core 模块是游戏的**初始化中心**，通过反射自动发现并初始化所有 `IGameSystem` 实现。

---

## 为什么用反射

### 不用反射（直接引用）

```csharp
// 每新增一个系统，都要改这里
private void InitializeAllSystems()
{
    var dataManager = new DataManager();
    dataManager.Initialize();

    var saveManager = new SaveManager();
    saveManager.Initialize();

    var audioSystem = new AudioSystem();     // 新增要改
    audioSystem.Initialize();

    var uiManager = new BaseUIManager();     // 新增要改
    uiManager.Initialize();
}
```

**问题：**
- 新增系统 → 必须改 Core 代码 + 加程序集引用
- 删除系统 → 必须改 Core 代码 + 删程序集引用
- 多人开发 → 都改 Core → 代码冲突
- 框架复用 → 新游戏缺系统 → 编译失败

### 用反射

```csharp
// Core 只依赖 IGameSystem 接口，不知道任何具体系统
// 新增系统只需创建类实现 IGameSystem，Core 自动发现
private void InitializeAllSystems()
{
    var types = DiscoverGameSystems();  // 反射扫描
    foreach (var type in types)
    {
        var system = Activator.CreateInstance(type) as IGameSystem;
        system.Initialize();
    }
}
```

**好处：**
- 新增系统 → 只需新建类实现 IGameSystem，Core 不用改
- 删除系统 → 删掉那个类就行，Core 不用改
- 多人开发 → 各写各的，不冲突
- 框架复用 → 缺的系统不会被扫描到，正常运行

### 核心价值

**反射的最大价值 = 不用改核心代码。** Core 只需要知道"我要找实现了 IGameSystem 的类"，不需要知道具体有哪些类。这就是**开闭原则**：对扩展开放，对修改关闭。

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
    public int Priority => 100;

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
       ├─► DiscoverGameSystems()  反射扫描所有程序集，查找 IGameSystem 实现
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
