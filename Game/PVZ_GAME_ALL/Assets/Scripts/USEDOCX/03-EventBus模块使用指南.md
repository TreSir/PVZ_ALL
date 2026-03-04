# EventBus 事件总线模块使用指南

## 概述

EventBus 是一个基于类型的事件发布/订阅系统，用于实现游戏模块间的松耦合通信。通过事件总线，一个模块可以发布事件而不需要知道谁在监听，监听者也可以订阅事件而不需要知道是谁发布的。

## 核心概念

### IEvent - 事件接口

所有事件类型都需要实现 IEvent 接口：

```csharp
using EventBus;

public struct MyEvent : IEvent
{
    public int Value;
    public string Message;
}
```

### EventBus - 事件总线

提供事件的订阅、取消订阅和发布功能：

```csharp
EventBus.Subscribe<MyEvent>(handler);    // 订阅
EventBus.Unsubscribe<MyEvent>(handler);  // 取消订阅
EventBus.Publish(new MyEvent());         // 发布
```

## 基本使用

### 1. 定义事件

```csharp
using EventBus;

// 使用结构体定义事件（推荐，减少GC）
public struct GameStartEvent : IEvent
{
    public int Level;
    public string PlayerName;
}

// 也可以使用类
public class PlayerDeathEvent : IEvent
{
    public int PlayerID;
    public Vector3 Position;
    public float Damage;
}
```

### 2. 订阅事件

```csharp
using UnityEngine;
using EventBus;

public class EventListener : MonoBehaviour
{
    private void OnEnable()
    {
        // 订阅事件
        EventBus.Subscribe<GameStartEvent>(OnGameStart);
        EventBus.Subscribe<PlayerDeathEvent>(OnPlayerDeath);
    }

    private void OnDisable()
    {
        // 取消订阅（重要！防止内存泄漏）
        EventBus.Unsubscribe<GameStartEvent>(OnGameStart);
        EventBus.Unsubscribe<PlayerDeathEvent>(OnPlayerDeath);
    }

    private void OnGameStart(GameStartEvent e)
    {
        Debug.Log($"游戏开始！关卡: {e.Level}, 玩家: {e.PlayerName}");
    }

    private void OnPlayerDeath(PlayerDeathEvent e)
    {
        Debug.Log($"玩家 {e.PlayerID} 在 {e.Position} 受到 {e.Damage} 点伤害后死亡");
    }
}
```

### 3. 发布事件

```csharp
using EventBus;

public class GameManager : MonoBehaviour
{
    private void StartGame()
    {
        // 发布游戏开始事件
        EventBus.Publish(new GameStartEvent
        {
            Level = 1,
            PlayerName = "Player1"
        });
    }

    private void OnPlayerHit(int playerID, float damage, Vector3 position)
    {
        // 发布玩家死亡事件
        if (damage >= 100)
        {
            EventBus.Publish(new PlayerDeathEvent
            {
                PlayerID = playerID,
                Position = position,
                Damage = damage
            });
        }
    }
}
```

## 高级功能

### 1. 事件优先级

订阅时可以指定优先级，优先级越高越先执行：

```csharp
// 优先级示例：数字越大越先执行
EventBus.Subscribe<GameStartEvent>(OnGameStartHighPriority, priority: 100);
EventBus.Subscribe<GameStartEvent>(OnGameStartNormal, priority: 0);
EventBus.Subscribe<GameStartEvent>(OnGameStartLowPriority, priority: -100);
```

### 2. 异步事件发布

对于耗时操作，可以使用异步发布：

```csharp
// 异步发布事件（在后台线程执行）
EventBus.PublishAsync(new PlayerDataSaveEvent { Data = "..." });

// 注意：异步事件处理器中不要操作 Unity 对象
```

### 3. 事件查询

```csharp
// 检查是否有订阅者
if (EventBus.HasHandler<GameStartEvent>())
{
    Debug.Log("有人订阅了 GameStartEvent");
}

// 获取订阅者数量
int count = EventBus.GetHandlerCount<GameStartEvent>();
Debug.Log($"GameStartEvent 有 {count} 个订阅者");
```

### 4. 清空事件

```csharp
// 清空所有事件订阅
EventBus.Clear();

// 清空特定事件订阅
EventBus.Clear<GameStartEvent>();
```

## 常见使用场景

### 场景1：玩家受伤事件

```csharp
// 定义事件
public struct PlayerHealthChangedEvent : IEvent
{
    public int PlayerID;
    public float CurrentHealth;
    public float MaxHealth;
    public float ChangeAmount;
}

// 订阅方 - UI 显示血条
public class HealthBarUI : MonoBehaviour
{
    private void OnEnable()
    {
        EventBus.Subscribe<PlayerHealthChangedEvent>(OnHealthChanged);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<PlayerHealthChangedEvent>(OnHealthChanged);
    }

    private void OnHealthChanged(PlayerHealthChangedEvent e)
    {
        // 更新血条显示
        float percent = e.CurrentHealth / e.MaxHealth;
        UpdateHealthBar(percent);
    }
}

// 发布方 - 玩家受伤时
public class Player : MonoBehaviour
{
    public void TakeDamage(float damage)
    {
        _health -= damage;

        EventBus.Publish(new PlayerHealthChangedEvent
        {
            PlayerID = _playerID,
            CurrentHealth = _health,
            MaxHealth = _maxHealth,
            ChangeAmount = -damage
        });

        if (_health <= 0)
        {
            EventBus.Publish(new PlayerDeathEvent { PlayerID = _playerID });
        }
    }
}
```

### 场景2：分数变化事件

```csharp
// 定义事件
public struct ScoreChangedEvent : IEvent
{
    public int NewScore;
    public int Delta;
}

// 订阅方 - 分数显示
public class ScoreDisplay : MonoBehaviour
{
    private void OnEnable() => EventBus.Subscribe<ScoreChangedEvent>(OnScoreChanged);
    private void OnDisable() => EventBus.Unsubscribe<ScoreChangedEvent>(OnScoreChanged);

    private void OnScoreChanged(ScoreChangedEvent e)
    {
        Debug.Log($"分数变化: +{e.Delta}, 当前分数: {e.NewScore}");
    }
}

// 订阅方 - 成就系统
public class AchievementSystem : MonoBehaviour
{
    private void OnEnable() => EventBus.Subscribe<ScoreChangedEvent>(CheckAchievements);
    private void OnDisable() => EventBus.Unsubscribe<ScoreChangedEvent>(CheckAchievements);

    private void CheckAchievements(ScoreChangedEvent e)
    {
        if (e.NewScore >= 1000)
            UnlockAchievement("Score1000");
        if (e.NewScore >= 5000)
            UnlockAchievement("Score5000");
    }
}
```

### 场景3：游戏状态事件

```csharp
// 游戏状态事件
public struct GameStateChangedEvent : IEvent
{
    public GameState NewState;
    public GameState PreviousState;
}

public enum GameState
{
    Menu,
    Playing,
    Paused,
    GameOver
}

// 发布方
public class GameStateManager : MonoBehaviour
{
    private GameState _currentState = GameState.Menu;

    public void ChangeState(GameState newState)
    {
        var previousState = _currentState;
        _currentState = newState;

        EventBus.Publish(new GameStateChangedEvent
        {
            NewState = newState,
            PreviousState = previousState
        });
    }
}

// 订阅方 - 音频系统
public class AudioGameStateHandler : MonoBehaviour
{
    private void OnEnable()
    {
        EventBus.Subscribe<GameStateChangedEvent>(OnStateChanged);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<GameStateChangedEvent>(OnStateChanged);
    }

    private void OnStateChanged(GameStateChangedEvent e)
    {
        switch (e.NewState)
        {
            case GameState.Playing:
                AudioManager.Instance.ResumeAll();
                break;
            case GameState.Paused:
                AudioManager.Instance.PauseAll();
                break;
            case GameState.GameOver:
                AudioManager.Instance.StopAll();
                break;
        }
    }
}
```

### 场景4：物品收集事件

```csharp
public struct ItemCollectedEvent : IEvent
{
    public string ItemID;
    public Vector3 Position;
    public int Quantity;
}

// 收集物品时发布
public class Collectible : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            EventBus.Publish(new ItemCollectedEvent
            {
                ItemID = _itemID,
                Position = transform.position,
                Quantity = _quantity
            });

            Destroy(gameObject);
        }
    }
}
```

## 最佳实践

### 1. 事件定义规范

```csharp
// ✅ 推荐：使用结构体
public struct PlayerEvent : IEvent
{
    public int PlayerID;
    public Vector3 Position;
}

// ✅ 可以：使用类（需要手动管理生命周期）
public class PlayerEventClass : IEvent
{
    public int PlayerID;
    public Vector3 Position;
}
```

### 2. 订阅/取消订阅配对

```csharp
// ✅ 正确：在 OnEnable 订阅，OnDisable 取消
private void OnEnable()
{
    EventBus.Subscribe<Event>(Handler);
}

private void OnDisable()
{
    EventBus.Unsubscribe<Event>(Handler);
}

// ❌ 错误：在 Start 订阅但不在 OnDestroy 取消
private void Start()
{
    EventBus.Subscribe<Event>(Handler);  // 可能导致内存泄漏
}

private void OnDestroy()
{
    // 忘记取消订阅
}
```

### 3. 事件命名规范

```csharp
// 使用过去式或命令式
public struct GameStartedEvent : IEvent { }   // ✅ 游戏开始事件
public struct ScoreChangedEvent : IEvent { }  // ✅ 分数变化事件
public struct OnPlayerHit : IEvent { }       // ⚠️ 命名不够清晰
```

### 4. 避免频繁事件

```csharp
// ❌ 不好：每帧发布事件
private void Update()
{
    EventBus.Publish(new PlayerPositionEvent { Position = transform.position });
}

// ✅ 好：只在位置真正变化时发布
private Vector3 _lastPosition;
private void Update()
{
    if (Vector3.Distance(transform.position, _lastPosition) > 0.01f)
    {
        _lastPosition = transform.position;
        EventBus.Publish(new PlayerPositionEvent { Position = _lastPosition });
    }
}
```

## 注意事项

1. **线程安全**：EventBus 使用 lock 保护，但建议在主线程发布事件
2. **内存泄漏**：务必在 OnDisable 或 OnDestroy 中取消订阅
3. **事件顺序**：相同优先级时，执行顺序不确定
4. **空值检查**：发布事件前确保有订阅者（使用 HasHandler）
5. **避免滥用**：不要把所有调用都用事件，频繁调用用直接方法更高效

## 相关文件

- [EventBus.cs](../EventSystem/EventBus/EventBus.cs) - 事件总线实现
- [IEvent.cs](../EventSystem/EventBus/IEvent.cs) - 事件接口定义
