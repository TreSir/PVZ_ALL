# Core 核心模块使用指南

## 概述

Core 模块是游戏的**初始化中心**，负责自动发现、注册和初始化所有游戏系统。通过反射机制，任何实现 `IGameSystem` 接口的类都会被自动发现并管理。

## 核心组件

### GameInitializer (游戏初始化器)

**位置：** `Core/GameInitializer.cs`

**职责：**
- 自动创建单例（DontDestroyOnLoad）
- 反射扫描所有程序集，发现 IGameSystem 实现
- 按 Priority 排序初始化系统
- 管理 Update/LateUpdate 回调
- 游戏关闭时逆序关闭所有系统

---

## 系统接口

### IGameSystem - 基础接口

```csharp
namespace GameBase
{
    public interface IGameSystem
    {
        /// <summary>
        /// 初始化优先级，数字越小越先初始化
        /// 建议：基础系统 100-500，业务系统 1000+
        /// </summary>
        int Priority { get; }

        /// <summary>
        /// 初始化方法，由 GameInitializer 自动调用
        /// </summary>
        void Initialize();

        /// <summary>
        /// 关闭方法，游戏退出时调用
        /// </summary>
        void Shutdown();
    }
}
```

### IGameSystemPreload - 预加载接口

```csharp
public interface IGameSystemPreload : IGameSystem
{
    /// <summary>
    /// 预加载方法，在所有 Initialize 完成后调用
    /// </summary>
    void Preload();
}
```

### IGameSystemUpdate - 帧更新接口

```csharp
public interface IGameSystemUpdate : IGameSystem
{
    /// <summary>
    /// 每帧更新方法
    /// </summary>
    void Update(float deltaTime);
}
```

### IGameSystemLateUpdate - 晚帧更新接口

```csharp
public interface IGameSystemLateUpdate : IGameSystem
{
    /// <summary>
    /// 晚帧更新方法，在所有 Update 后执行
    /// </summary>
    void LateUpdate(float deltaTime);
}
```

---

## 使用示例

### 1. 创建基础游戏系统

```csharp
using UnityEngine;
using GameBase;

namespace MyGame
{
    /// <summary>
    /// 游戏数据管理器
    /// </summary>
    public class GameDataManager : IGameSystem
    {
        public int Priority => 100;  // 高优先级，先初始化

        public void Initialize()
        {
            Debug.Log("[GameDataManager] 初始化");
            LoadGameData();
        }

        public void Shutdown()
        {
            Debug.Log("[GameDataManager] 关闭");
            SaveGameData();
        }

        private void LoadGameData() { }
        private void SaveGameData() { }
    }
}
```

### 2. 创建需要预加载的系统

```csharp
using UnityEngine;
using GameBase;

namespace MyGame
{
    /// <summary>
    /// 资源预加载系统
    /// </summary>
    public class ResourcePreloader : IGameSystem, IGameSystemPreload
    {
        public int Priority => 200;

        public void Initialize()
        {
            Debug.Log("[ResourcePreloader] 初始化完成");
        }

        public void Preload()
        {
            Debug.Log("[ResourcePreloader] 开始预加载资源");
            // 预加载常用资源
        }

        public void Shutdown()
        {
            Debug.Log("[ResourcePreloader] 关闭");
        }
    }
}
```

### 3. 创建需要每帧更新的系统

```csharp
using UnityEngine;
using GameBase;

namespace MyGame
{
    /// <summary>
    /// 游戏时间管理系统
    /// </summary>
    public class GameTimeManager : IGameSystem, IGameSystemUpdate
    {
        public int Priority => 300;

        private float _gameTime;
        private float _timeScale = 1f;

        public void Initialize()
        {
            _gameTime = 0f;
            Debug.Log("[GameTimeManager] 初始化");
        }

        public void Update(float deltaTime)
        {
            _gameTime += deltaTime * _timeScale;
        }

        public void Shutdown()
        {
            Debug.Log($"[GameTimeManager] 游戏时长: {_gameTime}秒");
        }

        public float GameTime => _gameTime;
    }
}
```

### 4. 完整的多接口系统

```csharp
using UnityEngine;
using GameBase;

namespace MyGame
{
    /// <summary>
    /// 完整的游戏系统示例
    /// </summary>
    public class CompleteGameSystem : IGameSystem,
        IGameSystemPreload,
        IGameSystemUpdate,
        IGameSystemLateUpdate
    {
        public int Priority => 500;

        public void Initialize()
        {
            Debug.Log("[CompleteGameSystem] Initialize");
        }

        public void Preload()
        {
            Debug.Log("[CompleteGameSystem] Preload");
        }

        public void Update(float deltaTime)
        {
            // 每帧更新
        }

        public void LateUpdate(float deltaTime)
        {
            // 晚帧更新
        }

        public void Shutdown()
        {
            Debug.Log("[CompleteGameSystem] Shutdown");
        }
    }
}
```

---

## 初始化流程

```
游戏启动
    │
    ▼
GameInitializer.Awake()
    │
    ├─► DiscoverGameSystems()     扫描所有程序集
    │       │
    │       └─► 发现所有 IGameSystem 实现类
    │
    ├─► RegisterSystem()          注册每个系统
    │       │
    │       └─► Activator.CreateInstance()
    │
    ▼
InitializeAllSystems()            按 Priority 升序初始化
    │
    ├─► system.Initialize()       依次调用
    │
    └─► preloadSystem.Preload()   预加载（如果有）
```

---

## 优先级建议

| 优先级范围 | 用途 | 示例 |
|-----------|------|------|
| 0-100 | 核心系统 | 数据持久化、配置加载 |
| 100-500 | 基础系统 | ResourceManager、ObjectPoolManager |
| 500-1000 | 业务系统 | GameTimeManager、InputManager |
| 1000+ | 功能系统 | **AudioSystem (1000)**、**BaseUIManager (2000)** |

---

## 当前已注册系统

| 系统 | Priority | 说明 |
|------|----------|------|
| AudioSystem | 1000 | 音频系统，播放启动BGM |
| BaseUIManager | 2000 | UI系统，打开启动面板 |

---

## 注意事项

1. **无参构造函数**：实现 IGameSystem 的类必须有公开的无参构造函数
2. **单例保护**：如果类同时是单例，构造函数中需要设置 `_instance = this`
3. **线程安全**：Initialize 和 Shutdown 在主线程调用
4. **Application.isPlaying**：Shutdown 时检查是否在播放模式
5. **不要手动调用**：让 GameInitializer 自动管理初始化

---

## 相关文件

- [GameInitializer.cs](../Core/GameInitializer.cs) - 初始化器实现
- [IGameSystem.cs](../GameBase/IGameSystem.cs) - 接口定义
