# Audio 音频模块使用指南

## 概述

Audio 模块负责游戏中所有音频的播放和管理，包括背景音乐（BGM）和音效（SFX）。

---

## 架构设计

```
AudioSystem (IGameSystem)
     │
     └─► AudioManager (MonoBehaviour 单例)
              │
              ├─► BGM 播放（单声道）
              ├─► SFX 播放（对象池）
              └─► 音量控制
```

---

## 基本使用

### 播放背景音乐

```csharp
using Audio;

var clip = Resources.Load<AudioClip>("Audio/BGM/MainMenu");
AudioManager.Instance.PlayBGM(clip);

AudioManager.Instance.StopBGM();
AudioManager.Instance.PauseBGM();
AudioManager.Instance.ResumeBGM();
```

### 播放音效

```csharp
AudioManager.Instance.PlaySFX(clip);

AudioManager.Instance.PlaySFXAtPosition(clip, position);
```

---

## 音量控制

```csharp
AudioManager.Instance.MasterVolume = 0.8f;
AudioManager.Instance.BGMVolume = 0.5f;
AudioManager.Instance.SFXVolume = 1.0f;
```

---

## 配置说明

**位置：** `Resources/Configs/GameSettingConfig.asset`

| 配置项 | 说明 |
|--------|------|
| masterVolume | 主音量 (0-1) |
| bgmVolume | BGM音量 (0-1) |
| sfxVolume | SFX音量 (0-1) |
| startupBGMPath | 启动BGM路径 |
| audioInitialPoolSize | 音频池初始大小 |
| audioMaxPoolSize | 音频池最大大小 |

---

## 目录结构

```
Resources/
└── Audio/
    ├── BGM/
    │   ├── MainMenu.ogg
    │   └── Game.ogg
    └── SE/
        ├── Click.ogg
        └── Jump.ogg
```

---

## API 速查

| 方法 | 说明 |
|------|------|
| `PlayBGM(clip)` | 播放背景音乐 |
| `StopBGM()` | 停止BGM |
| `PauseBGM()` | 暂停BGM |
| `ResumeBGM()` | 恢复BGM |
| `PlaySFX(clip)` | 播放音效 |
| `PlaySFXAtPosition(clip, pos)` | 播放3D音效 |
