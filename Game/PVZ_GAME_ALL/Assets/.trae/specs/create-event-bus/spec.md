# 事件总线 Spec

## Why
游戏需要一个解耦的事件通信系统，让不同模块之间可以通过事件进行通信，避免直接依赖，降低模块间耦合度。

## What Changes
- 在 `Scripts/EventBus` 文件夹下创建事件总线模块
- 实现 IEvent 接口，定义事件基类
- 实现 EventBus 静态类，提供订阅、发布、取消订阅功能
- 创建 EventBus.asmdef 程序集定义文件

## Impact
- Affected specs: 无，新增模块
- Affected code: 新增 Scripts/EventBus 文件夹

## ADDED Requirements

### Requirement: EventBus Core
The system SHALL provide a static EventBus for event-based communication.

#### Scenario: Subscribe and Publish
- **WHEN** 模块A 订阅某个事件
- **AND** 模块B 发布该事件
- **THEN** 模块A 收到事件通知

#### Scenario: Unsubscribe
- **WHEN** 模块取消订阅事件
- **THEN** 后续发布的事件不再通知该模块

### Requirement: Type-safe Events
The system SHALL support type-safe event handling.

#### Scenario: Generic Event
- **WHEN** 定义一个事件类型实现 IEvent
- **THEN** 订阅者可以安全地接收该类型的事件数据

### Requirement: Memory Safety
The system SHALL prevent memory leaks from event subscriptions.

#### Scenario: Clear All Subscriptions
- **WHEN** 调用清除方法
- **THEN** 所有事件订阅被清除
