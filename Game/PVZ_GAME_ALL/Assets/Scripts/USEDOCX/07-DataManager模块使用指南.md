# DataManager 数据管理模块使用指南

## 概述

DataManager 模块负责游戏数据的统一管理，包括配置数据加载、运行时数据管理和存档系统。采用模块化设计，职责清晰分离。

## 架构设计

```
┌─────────────────────────────────────────────────────────────┐
│                    GameInitializer                          │
│                  (自动发现 IGameSystem)                      │
└─────────────────────────┬───────────────────────────────────┘
                          │
         ┌────────────────┴────────────────┐
         │                                 │
         ▼                                 ▼
┌─────────────────┐              ┌─────────────────┐
│   DataManager   │              │   SaveManager   │
│  (Priority: 50) │              │  (Priority: 51) │
│                 │              │                 │
│ - 配置数据加载   │              │ - 存档槽管理    │
│ - 运行时数据     │              │ - 自动保存      │
│ - 数据缓存      │              │ - 存档读写      │
└─────────────────┘              └────────┬────────┘
                                          │
                                          ▼
                                ┌─────────────────┐
                                │ Serialization   │
                                │    Utility      │
                                │                 │
                                │ - JSON序列化    │
                                │ - 文件读写      │
                                └─────────────────┘
```

---

## 一、DataManager - 配置与运行时数据

### 1.1 加载配置数据

```csharp
using DataManager;
using UnityEngine;

public class GameLoader : MonoBehaviour
{
    void Start()
    {
        var zombieConfig = DataManager.Instance.LoadConfig<ZombieConfig>("Configs/ZombieConfig");
        var plantConfig = DataManager.Instance.LoadConfig<PlantConfig>("Configs/PlantConfig");
    }
}
```

**配置文件位置：** `Resources/Configs/` 目录下

### 1.2 运行时数据管理

```csharp
using DataManager;

public class LevelController : MonoBehaviour
{
    void OnEnterLevel(int levelId)
    {
        var levelData = new LevelTempData { levelId = levelId, currentSun = 50 };
        DataManager.Instance.Register("LevelData", levelData);
    }

    void AddSun(int amount)
    {
        if (DataManager.Instance.TryGet<LevelTempData>("LevelData", out var data))
        {
            data.currentSun += amount;
        }
    }

    void OnExitLevel()
    {
        DataManager.Instance.Unregister("LevelData");
    }
}
```

---

## 二、SaveManager - 存档系统

### 2.1 存档槽管理

```csharp
using DataManager;

public class SaveMenu : MonoBehaviour
{
    void ShowSaveSlots()
    {
        var slots = SaveManager.Instance.GetAllSaveSlotInfos();
        foreach (var slot in slots)
        {
            Debug.Log($"槽位 {slot.slotIndex}: {(slot.isEmpty ? "空" : slot.lastSaveTime)}");
        }
    }

    void SelectSlot(int index)
    {
        SaveManager.Instance.SelectSlot(index);
    }
}
```

### 2.2 保存与加载

```csharp
using DataManager;

public class GameController : MonoBehaviour
{
    void SaveGame()
    {
        var saveData = new GameSaveData
        {
            playTime = totalPlayTime,
            levelProgress = currentLevel
        };
        SaveManager.Instance.SaveCurrentData(saveData);
    }

    void LoadGame()
    {
        var saveData = SaveManager.Instance.GetCurrentSaveData<GameSaveData>();
        totalPlayTime = saveData.playTime;
        currentLevel = saveData.levelProgress;
    }
}
```

### 2.3 自动保存

SaveManager 会根据配置自动保存：

```csharp
[Serializable]
public class DataSettings
{
    public bool enableCache = true;
    public string persistentFolderName = "GameData";
    public float autoSaveInterval = 60f;
}
```

---

## 三、自定义存档数据

### 3.1 创建存档数据类

```csharp
using System;
using DataManager;

[Serializable]
public class GameSaveData : ISaveData
{
    public string lastSaveTime { get; set; }
    public float playTime { get; set; }

    public int levelProgress;
    public int totalSun;
    public List<string> unlockedPlants;
}
```

### 3.2 ISaveData 接口

```csharp
public interface ISaveData
{
    string lastSaveTime { get; set; }
    float playTime { get; set; }
}
```

---

## 四、配置说明

### GameSettingConfig 数据设置

```csharp
[Serializable]
public class DataSettings
{
    [Tooltip("是否启用配置数据缓存")]
    public bool enableCache = true;

    [Tooltip("持久化数据存储目录名")]
    public string persistentFolderName = "GameData";

    [Tooltip("自动保存间隔（秒），0表示禁用")]
    public float autoSaveInterval = 60f;
}
```

---

## 五、API 速查

### DataManager

| 方法 | 说明 |
|------|------|
| `LoadConfig<T>(path)` | 加载配置（自动缓存） |
| `Register<T>(key, data)` | 注册运行时数据 |
| `Get<T>(key)` | 获取运行时数据 |
| `TryGet<T>(key, out data)` | 尝试获取数据 |
| `Unregister(key)` | 移除运行时数据 |
| `ClearConfigCache()` | 清空配置缓存 |

### SaveManager

| 方法 | 说明 |
|------|------|
| `SelectSlot(index)` | 选择存档槽 |
| `GetAllSaveSlotInfos()` | 获取所有存档信息 |
| `GetCurrentSaveData<T>()` | 获取当前存档数据 |
| `SaveCurrentData<T>(data)` | 保存到当前槽 |
| `LoadSaveData<T>(slotIndex)` | 加载指定槽数据 |
| `DeleteSave(slotIndex)` | 删除存档 |

---

## 六、文件存储位置

```
Application.persistentDataPath/
└── GameData/
    ├── slot_0/
    │   ├── GameSaveData.json
    │   └── SlotInfo.json
    ├── slot_1/
    └── slot_2/
```
