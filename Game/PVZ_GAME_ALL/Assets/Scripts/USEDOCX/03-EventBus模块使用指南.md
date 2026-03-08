# EventBus 事件总线模块使用指南

## 概述

EventBus 是基于类型的事件发布/订阅系统，用于模块间松耦合通信。

---

## 核心概念

### 定义事件

```csharp
using EventBus;

public struct GameStartEvent : IEvent
{
    public int Level;
    public string PlayerName;
}
```

---

## 基本使用

### 订阅事件

```csharp
using EventBus;

void OnEnable()
{
    EventBus.Subscribe<GameStartEvent>(OnGameStart);
}

void OnDisable()
{
    EventBus.Unsubscribe<GameStartEvent>(OnGameStart);
}

void OnGameStart(GameStartEvent e)
{
    Debug.Log($"游戏开始: {e.Level}");
}
```

### 发布事件

```csharp
EventBus.Publish(new GameStartEvent
{
    Level = 1,
    PlayerName = "Player1"
});
```

---

## 高级功能

### 优先级

```csharp
EventBus.Subscribe<GameStartEvent>(OnHigh, priority: 100);
EventBus.Subscribe<GameStartEvent>(OnLow, priority: -100);
```

### 异步发布

```csharp
EventBus.PublishAsync(new SaveEvent());
```

### 查询

```csharp
bool hasHandler = EventBus.HasHandler<GameStartEvent>();
int count = EventBus.GetHandlerCount<GameStartEvent>();
```

---

## API 速查

| 方法 | 说明 |
|------|------|
| `Subscribe<T>(handler)` | 订阅事件 |
| `Unsubscribe<T>(handler)` | 取消订阅 |
| `Publish<T>(event)` | 发布事件 |
| `PublishAsync<T>(event)` | 异步发布 |
| `HasHandler<T>()` | 是否有订阅者 |
| `Clear()` | 清空所有订阅 |

---

## 注意事项

- 使用 struct 定义事件可减少 GC
- OnDisable 时必须取消订阅
- 异步事件中不要操作 Unity 对象
