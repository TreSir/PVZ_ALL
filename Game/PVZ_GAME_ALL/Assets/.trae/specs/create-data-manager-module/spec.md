# DataManager 数据管理模块 Spec

## Why

当前游戏架构缺少统一的数据管理模块。需要一个通用的数据管理器来负责数据的加载、管理、序列化和反序列化。支持从配置文件加载数据并进行统一管理，使用 Newtonsoft JSON 进行序列化操作。

## What Changes

* 创建 DataManager 模块，实现 IGameSystem 接口

* 提供通用的数据加载和管理功能

* 提供数据的序列化和反序列化功能（使用 Newtonsoft JSON）

* 支持从配置文件（ScriptableObject、JSON等）加载数据

* 提供数据缓存机制，避免重复加载

* 在 GameSettingConfig 中添加数据管理相关配置

## Impact

* Affected specs: GameSettingConfig 需要添加 DataSettings 配置类

* Affected code:

  * 新增 Scripts/DataManager/ 目录及所有文件

  * 修改 Config/GameSettingConfig.cs 添加数据配置

* Priority: 建议设置为 50（在基础系统之前初始化）

## 架构设计

```
┌─────────────────────────────────────────────────────────────────────────┐
│                          DataManager                                     │
│                  (IGameSystem, Priority: 50)                             │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                          │
│  ┌────────────────────────────────────────────────────────────────────┐ │
│  │                        数据加载 (Data Loading)                      │ │
│  │  - Load<T>(path): 从 Resources 加载数据                            │ │
│  │  - LoadConfig<T>(): 加载 ScriptableObject 配置                     │ │
│  │  - LoadJson<T>(path): 加载 JSON 文件                               │ │
│  └────────────────────────────────────────────────────────────────────┘ │
│                                                                          │
│  ┌────────────────────────────────────────────────────────────────────┐ │
│  │                        数据管理 (Data Management)                   │ │
│  │  - Register<T>(key, data): 注册数据到缓存                          │ │
│  │  - Get<T>(key): 从缓存获取数据                                     │ │
│  │  - Unregister(key): 移除缓存数据                                   │ │
│  │  - Clear(): 清空所有缓存                                           │ │
│  └────────────────────────────────────────────────────────────────────┘ │
│                                                                          │
│  ┌────────────────────────────────────────────────────────────────────┐ │
│  │                    序列化/反序列化 (Serialization)                  │ │
│  │  - Serialize<T>(data): 序列化对象为 JSON 字符串                    │ │
│  │  - Deserialize<T>(json): 反序列化 JSON 字符串为对象                │ │
│  │  - SaveToFile<T>(data, path): 序列化并保存到文件                   │ │
│  │  - LoadFromFile<T>(path): 从文件加载并反序列化                     │ │
│  └────────────────────────────────────────────────────────────────────┘ │
│                                                                          │
│  ┌────────────────────────────────────────────────────────────────────┐ │
│  │                        数据缓存 (Data Cache)                        │ │
│  │  - Dictionary<string, object> _dataCache                           │ │
│  │  - 缓存已加载的数据，避免重复加载                                   │ │
│  └────────────────────────────────────────────────────────────────────┘ │
│                                                                          │
└─────────────────────────────────────────────────────────────────────────┘
```

## ADDED Requirements

### Requirement: 数据加载功能

系统 SHALL 提供多种数据加载方式。

#### Scenario: 从 Resources 加载数据

* **WHEN** 调用 Load<T>(path) 方法

* **THEN** 从 Resources 目录加载指定类型的资源

* **AND** 自动缓存加载的数据

#### Scenario: 加载 ScriptableObject 配置

* **WHEN** 调用 LoadConfig<T>() 方法

* **THEN** 加载 ScriptableObject 类型的配置数据

* **AND** 返回配置实例

#### Scenario: 加载 JSON 文件

* **WHEN** 调用 LoadJson<T>(path) 方法

* **THEN** 从 Resources 加载 JSON 文件

* **AND** 使用 Newtonsoft JSON 反序列化为指定类型

* **AND** 自动缓存数据

### Requirement: 数据管理功能

系统 SHALL 提供数据的注册、获取和移除功能。

#### Scenario: 注册数据

* **WHEN** 调用 Register<T>(key, data) 方法

* **THEN** 将数据注册到缓存中

* **AND** 使用指定的 key 作为标识

#### Scenario: 获取数据

* **WHEN** 调用 Get<T>(key) 方法

* **THEN** 从缓存中获取指定 key 的数据

* **AND** 如果数据不存在返回默认值

#### Scenario: 移除数据

* **WHEN** 调用 Unregister(key) 方法

* **THEN** 从缓存中移除指定 key 的数据

#### Scenario: 清空缓存

* **WHEN** 调用 Clear() 方法

* **THEN** 清空所有缓存数据

### Requirement: 序列化功能

系统 SHALL 使用 Newtonsoft JSON 提供序列化和反序列化功能。

#### Scenario: 序列化对象

* **WHEN** 调用 Serialize<T>(data) 方法

* **THEN** 使用 Newtonsoft JSON 将对象序列化为 JSON 字符串

* **AND** 返回 JSON 字符串

#### Scenario: 反序列化对象

* **WHEN** 调用 Deserialize<T>(json) 方法

* **THEN** 使用 Newtonsoft JSON 将 JSON 字符串反序列化为指定类型对象

* **AND** 返回对象实例

#### Scenario: 保存到文件

* **WHEN** 调用 SaveToFile<T>(data, path) 方法

* **THEN** 序列化对象并保存到指定路径的文件

* **AND** 支持持久化目录（Application.persistentDataPath）

#### Scenario: 从文件加载

* **WHEN** 调用 LoadFromFile<T>(path) 方法

* **THEN** 从指定路径的文件读取 JSON

* **AND** 反序列化为指定类型对象

### Requirement: 配置集成

系统 SHALL 从 GameSettingConfig 读取数据管理配置。

#### Scenario: 配置加载

* **WHEN** DataManager 初始化

* **THEN** 从 GameSettingConfig 读取 DataSettings

## MODIFIED Requirements

### Requirement: GameSettingConfig 扩展

GameSettingConfig SHALL 包含 DataSettings 配置类。

```csharp
[System.Serializable]
public class DataSettings
{
    [Tooltip("是否启用数据缓存")]
    public bool enableCache = true;
    
    [Tooltip("持久化数据存储目录名")]
    public string persistentFolderName = "GameData";
}
```

## REMOVED Requirements

无移除的需求。

## 文件结构

```
Scripts/
└── DataManager/
    ├── DataManager.cs           # 主管理器，实现 IGameSystem
    ├── DataManager.asmdef       # 程序集定义
    └── IDataSerializer.cs       # 序列化接口（可选，方便扩展）
```

## 依赖关系

```
DataManager
    ├── GameBase (IGameSystem)
    ├── Config (GameSettingConfig)
    └── Newtonsoft JSON (序列化库)
```

## 使用示例

### 加载数据

```csharp
// 从 Resources 加载配置
var zombieConfig = DataManager.Instance.Load<ZombieConfig>("Configs/ZombieConfig");

// 从 JSON 文件加载数据列表
var zombieDataList = DataManager.Instance.LoadJson<List<ZombieData>>("Data/Zombies");

// 加载 ScriptableObject 配置
var gameConfig = DataManager.Instance.LoadConfig<GameSettingConfig>();
```

### 数据管理

```csharp
// 注册数据
DataManager.Instance.Register("CurrentLevel", levelData);

// 获取数据
var levelData = DataManager.Instance.Get<LevelData>("CurrentLevel");

// 移除数据
DataManager.Instance.Unregister("CurrentLevel");

// 清空缓存
DataManager.Instance.Clear();
```

### 序列化/反序列化

```csharp
// 序列化对象
string json = DataManager.Instance.Serialize(playerData);

// 反序列化
var playerData = DataManager.Instance.Deserialize<PlayerData>(json);

// 保存到文件
DataManager.Instance.SaveToFile(saveData, "savegame.json");

// 从文件加载
var saveData = DataManager.Instance.LoadFromFile<SaveData>("savegame.json");
```

## 优先级说明

DataManager 的 Priority 设置为 50，原因：

1. 数据管理是基础服务，其他系统可能依赖数据
2. 需要在其他业务系统之前初始化

## Newtonsoft JSON 集成说明

需要通过 Unity Package Manager 或 UPM 安装 Newtonsoft JSON：

* 包名：`com.unity.nuget.newtonsoft-json`

* 或从 Unity Asset Store 下载 Json.NET for Unity

## API 概览

```csharp
namespace DataManager
{
    public class DataManager : IGameSystem
    {
        public int Priority => 50;
        
        // 数据加载
        public T Load<T>(string path) where T : UnityEngine.Object;
        public T LoadConfig<T>() where T : ScriptableObject;
        public T LoadJson<T>(string path);
        
        // 数据管理
        public void Register<T>(string key, T data);
        public T Get<T>(string key);
        public void Unregister(string key);
        public void Clear();
        
        // 序列化
        public string Serialize<T>(T data);
        public T Deserialize<T>(string json);
        public void SaveToFile<T>(T data, string fileName);
        public T LoadFromFile<T>(string fileName);
        public bool FileExists(string fileName);
        public void DeleteFile(string fileName);
        
        // IGameSystem
        public void Initialize();
        public void Shutdown();
    }
}
```

