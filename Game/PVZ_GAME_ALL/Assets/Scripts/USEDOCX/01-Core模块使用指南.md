# Core 核心模块使用指南

## 概述

Core 模块是游戏的核心初始化系统，提供统一的系统接口和自动发现机制。通过实现 `IGameSystem` 接口，任何游戏系统都可以被 GameInitializer 自动管理。

## 系统接口

### IGameSystem - 基础接口

```csharp
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
    /// 关闭方法，游戏退出或切换场景时调用
    /// </summary>
    void Shutdown();
}
```

### IGameSystemPreload - 预加载接口

```csharp
public interface IGameSystemPreload : IGameSystem
{
    /// <summary>
    /// 预加载方法，在所有 Initialize 完成后调用
    /// 用于预加载资源、预热对象池等
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
    /// <param name="deltaTime">距离上一帧的时间（秒）</param>
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
    /// <param name="deltaTime">距离上一帧的时间（秒）</param>
    void LateUpdate(float deltaTime);
}
```

## 使用示例

### 1. 创建基础游戏系统

```csharp
using UnityEngine;
using Core;

namespace MyGame
{
    /// <summary>
    /// 游戏数据管理器示例
    /// </summary>
    public class GameDataManager : MonoBehaviour, IGameSystem
    {
        // 优先级：数值越小越先初始化
        public int Priority => 100;

        public void Initialize()
        {
            Debug.Log("[GameDataManager] 初始化");
            // 加载存档数据
            LoadGameData();
        }

        public void Shutdown()
        {
            Debug.Log("[GameDataManager] 关闭");
            // 保存数据
            SaveGameData();
        }

        private void LoadGameData()
        {
            // 加载游戏数据逻辑
        }

        private void SaveGameData()
        {
            // 保存游戏数据逻辑
        }
    }
}
```

### 2. 创建需要预加载的系统

```csharp
using UnityEngine;
using Core;

namespace MyGame
{
    /// <summary>
    /// 资源预加载系统示例
    /// </summary>
    public class ResourcePreloader : MonoBehaviour, IGameSystem, IGameSystemPreload
    {
        public int Priority => 200;

        public void Initialize()
        {
            Debug.Log("[ResourcePreloader] 初始化完成");
        }

        /// <summary>
        /// 预加载资源，在 Initialize 全部完成后调用
        /// </summary>
        public void Preload()
        {
            Debug.Log("[ResourcePreloader] 开始预加载资源");
            // 预加载常用资源
            Resources.LoadAll<AudioClip>("Audio");
            Resources.LoadAll<GameObject>("Prefabs/Characters");
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
using Core;

namespace MyGame
{
    /// <summary>
    /// 游戏时间管理系统示例
    /// </summary>
    public class GameTimeManager : MonoBehaviour, IGameSystem, IGameSystemUpdate
    {
        public int Priority => 300;

        private float _gameTime;
        private float _timeScale = 1f;

        public void Initialize()
        {
            _gameTime = 0f;
            Debug.Log("[GameTimeManager] 初始化");
        }

        /// <summary>
        /// 每帧更新
        /// </summary>
        public void Update(float deltaTime)
        {
            _gameTime += deltaTime * _timeScale;
        }

        public void Shutdown()
        {
            Debug.Log($"[GameTimeManager] 游戏时长: {_gameTime}");
        }

        public float GameTime => _gameTime;
        public float TimeScale
        {
            get => _timeScale;
            set => _timeScale = Mathf.Clamp(value, 0f, 10f);
        }
    }
}
```

### 4. 完整的多系统示例

```csharp
using UnityEngine;
using Core;

namespace MyGame
{
    /// <summary>
    /// 完整的游戏系统示例
    /// </summary>
    public class CompleteGameSystem : MonoBehaviour,
        IGameSystem,
        IGameSystemPreload,
        IGameSystemUpdate,
        IGameSystemLateUpdate
    {
        // 优先级设置（越小越先初始化）
        public int Priority => 500;

        #region IGameSystem

        public void Initialize()
        {
            Debug.Log("[CompleteGameSystem] Initialize");
            // 1. 加载配置
            LoadConfig();
            // 2. 初始化数据
            InitializeData();
        }

        public void Shutdown()
        {
            Debug.Log("[CompleteGameSystem] Shutdown");
            // 保存数据
            SaveData();
        }

        #endregion

        #region IGameSystemPreload

        public void Preload()
        {
            Debug.Log("[CompleteGameSystem] Preload");
            // 预加载资源
        }

        #endregion

        #region IGameSystemUpdate

        public void Update(float deltaTime)
        {
            // 每帧更新逻辑
        }

        #endregion

        #region IGameSystemLateUpdate

        public void LateUpdate(float deltaTime)
        {
            // 晚帧更新逻辑
        }

        #endregion

        #region Private Methods

        private void LoadConfig()
        {
        }

        private void InitializeData()
        {
        }

        private void SaveData()
        {
        }

        #endregion
    }
}
```

## 初始化流程

```
1. GameInitializer.Awake()
       │
       ▼
2. 扫描所有程序集，发现所有 IGameSystem 实现类
       │
       ▼
3. 创建系统实例并注册
       │
       ▼
4. 按 Priority 升序调用所有系统的 Initialize()
       │
       ▼
5. 按 Priority 升序调用所有系统的 Preload()（如果有）
       │
       ▼
6. 游戏运行中...
       │
       ▼
7. 游戏退出时按 Priority 降序调用所有系统的 Shutdown()
```

## 优先级建议

| 优先级范围 | 用途 | 示例 |
|-----------|------|------|
| 0-100 | 核心系统 | 数据持久化、配置加载 |
| 100-500 | 基础系统 | ResourceManager、ObjectPoolManager |
| 500-1000 | 业务系统 | GameTimeManager、InputManager |
| 1000+ | 功能系统 | AudioManager、UIManager |

## 注意事项

1. **单例模式**：GameInitializer 本身是单例，会在场景开始时自动创建
2. **自动发现**：所有实现 IGameSystem 的类都会被自动发现，无需手动注册
3. **依赖关系**：如果系统之间有依赖，需要合理设置 Priority
4. **线程安全**：Initialize 和 Shutdown 会在主线程调用，无需担心线程安全问题
5. **场景切换**：当前实现不会自动处理场景切换，如需处理需要在 Shutdown 中自行处理

## 相关文件

- [IGameSystem.cs](../Core/IGameSystem.cs) - 接口定义
- [GameInitializer.cs](../Core/GameInitializer.cs) - 初始化器实现
