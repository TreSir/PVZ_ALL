# Audio 音频模块使用指南

## 概述

Audio 模块负责游戏中所有音频的播放和管理，包括背景音乐（BGM）和音效（SFX）。该模块采用**包装器模式**，通过 `AudioSystem` 实现 `IGameSystem` 接口，由 `GameInitializer` 自动发现和初始化。

## 架构设计

```
┌─────────────────────────────────────────────────────────────┐
│                    GameInitializer                          │
│                  (自动发现 IGameSystem)                      │
└─────────────────────────┬───────────────────────────────────┘
                          │
                          ▼
┌─────────────────────────────────────────────────────────────┐
│                      AudioSystem                            │
│              (IGameSystem, Priority: 1000)                  │
│                                                             │
│  Initialize() ──► AudioManager.Instance.Initialize()        │
│  Shutdown()  ──► AudioManager.Instance.Shutdown()           │
└─────────────────────────┬───────────────────────────────────┘
                          │
                          ▼
┌─────────────────────────────────────────────────────────────┐
│                     AudioManager                            │
│              (MonoBehaviour 单例)                           │
│                                                             │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────────────┐ │
│  │  BGM 播放   │  │  SFX 播放   │  │  ObjectPool 管理    │ │
│  │  (单声道)   │  │  (池化)     │  │  (AudioSource)      │ │
│  └─────────────┘  └─────────────┘  └─────────────────────┘ │
└─────────────────────────────────────────────────────────────┘
```

---

## 配置说明

### GameSettingConfig 配置

**位置：** `Resources/Configs/GameSettingConfig.asset`

```csharp
[Header("Audio Settings")]
public class AudioSettings
{
    [Tooltip("主音量 (0-1)")]
    [Range(0f, 1f)]
    public float masterVolume = 1f;

    [Tooltip("背景音乐音量 (0-1)")]
    [Range(0f, 1f)]
    public float bgmVolume = 1f;

    [Tooltip("音效音量 (0-1)")]
    [Range(0f, 1f)]
    public float sfxVolume = 1f;

    [Tooltip("游戏启动时播放的背景音乐路径")]
    public string startupBGMPath = "Audio/BGM/B_001";
}

[Header("Pool Settings")]
public class PoolSettings
{
    [Tooltip("AudioSource 池初始大小")]
    [Min(1)]
    public int audioInitialPoolSize = 10;

    [Tooltip("AudioSource 池最大大小")]
    [Min(1)]
    public int audioMaxPoolSize = 30;
}
```

---

## 游戏启动流程

```
游戏启动
    │
    ▼
GameInitializer.Awake()
    │
    ▼
AudioSystem.Initialize() (Priority: 1000)
    │
    ▼
AudioManager.Instance (自动创建单例)
    │
    ├─► new GameObject("[AudioManager]")
    ├─► AddComponent<AudioManager>()
    └─► DontDestroyOnLoad()
    │
    ▼
AudioManager.Initialize()
    │
    ├─► 加载 GameSettingConfig
    ├─► 设置音量 (masterVolume, bgmVolume, sfxVolume)
    ├─► 初始化 AudioSource 对象池
    │
    └─► 播放启动 BGM (startupBGMPath)
        │
        ▼
    [AudioManager] Playing startup BGM: Audio/BGM/B_001
```

---

## 基础使用

### 播放背景音乐

```csharp
using UnityEngine;
using Audio;

public class GameExample : MonoBehaviour
{
    public void PlayMainMenuBGM()
    {
        var clip = Resources.Load<AudioClip>("Audio/BGM/MainMenu");
        if (clip != null)
        {
            AudioManager.Instance.PlayBGM(clip);
        }
    }

    public void PlayGameBGM()
    {
        var clip = Resources.Load<AudioClip>("Audio/BGM/GameLevel");
        if (clip != null)
        {
            AudioManager.Instance.PlayBGM(clip);
        }
    }

    public void StopBGM()
    {
        AudioManager.Instance.StopBGM();
    }

    public void PauseBGM()
    {
        AudioManager.Instance.PauseBGM();
    }

    public void ResumeBGM()
    {
        AudioManager.Instance.ResumeBGM();
    }
}
```

### 播放音效

```csharp
using UnityEngine;
using Audio;

public class PlayerController : MonoBehaviour
{
    public AudioClip jumpClip;
    public AudioClip attackClip;
    public AudioClip hitClip;

    public void OnJump()
    {
        if (jumpClip != null)
        {
            AudioManager.Instance.PlaySFX(jumpClip);
        }
    }

    public void OnAttack()
    {
        if (attackClip != null)
        {
            AudioManager.Instance.PlaySFX(attackClip);
        }
    }

    public void OnHit(Vector3 position)
    {
        if (hitClip != null)
        {
            // 播放 3D 音效（指定位置）
            AudioManager.Instance.PlaySFXAtPosition(hitClip, position);
        }
    }
}
```

---

## 音量控制

### 单独控制音量

```csharp
using Audio;

public class SettingsMenu : MonoBehaviour
{
    public void SetMasterVolume(float value)
    {
        AudioManager.Instance.MasterVolume = value;  // 0-1
    }

    public void SetBGMVolume(float value)
    {
        AudioManager.Instance.BGMVolume = value;  // 0-1
    }

    public void SetSFXVolume(float value)
    {
        AudioManager.Instance.SFXVolume = value;  // 0-1
    }

    public float GetMasterVolume() => AudioManager.Instance.MasterVolume;
    public float GetBGMVolume() => AudioManager.Instance.BGMVolume;
    public float GetSFXVolume() => AudioManager.Instance.SFXVolume;
}
```

### 批量设置音量

```csharp
using Audio;

public class AudioSettings : MonoBehaviour
{
    public void ApplySettings(float master, float bgm, float sfx)
    {
        AudioManager.Instance.SetVolumeSettings(master, bgm, sfx);
    }
}
```

---

## 高级功能

### 使用 SoundConfig 自定义播放

```csharp
using UnityEngine;
using Audio;

public class AdvancedAudioExample : MonoBehaviour
{
    public AudioClip specialSFX;

    public void PlaySpecialSound()
    {
        var config = new SoundConfig
        {
            volume = 0.8f,
            pitch = 1.2f,        // 音调
            loop = false,
            spatialBlend = 0f,   // 0=2D, 1=3D
            priority = 128       // 优先级 (0=最高)
        };

        AudioManager.Instance.PlaySFX(specialSFX, config);
    }

    public void Play3DSound(Vector3 position)
    {
        var config = new SoundConfig
        {
            volume = 1.0f,
            pitch = 1.0f,
            loop = false,
            spatialBlend = 1f,   // 完全 3D
            priority = 128
        };

        AudioManager.Instance.PlaySFXAtPosition(specialSFX, position, config);
    }
}
```

### 全局控制

```csharp
using Audio;

public class PauseManager : MonoBehaviour
{
    public void PauseGame()
    {
        Time.timeScale = 0f;
        AudioManager.Instance.PauseAll();  // 暂停所有音频
    }

    public void ResumeGame()
    {
        Time.timeScale = 1f;
        AudioManager.Instance.ResumeAll();  // 恢复所有音频
    }

    public void StopAllAudio()
    {
        AudioManager.Instance.StopAll();  // 停止所有音频
    }
}
```

---

## 与 EventBus 集成

```csharp
using UnityEngine;
using Audio;
using EventBus;

// 定义音频事件
public struct PlaySFXEvent : IEvent
{
    public string ClipPath;
    public Vector3 Position;
}

public class AudioEventSystem : MonoBehaviour
{
    private void OnEnable()
    {
        EventBus.Subscribe<PlaySFXEvent>(OnPlaySFX);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<PlaySFXEvent>(OnPlaySFX);
    }

    private void OnPlaySFX(PlaySFXEvent e)
    {
        var clip = Resources.Load<AudioClip>(e.ClipPath);
        if (clip != null)
        {
            if (e.Position != Vector3.zero)
            {
                AudioManager.Instance.PlaySFXAtPosition(clip, e.Position);
            }
            else
            {
                AudioManager.Instance.PlaySFX(clip);
            }
        }
    }
}

// 使用示例
public class Enemy : MonoBehaviour
{
    public void OnDeath()
    {
        EventBus.Publish(new PlaySFXEvent
        {
            ClipPath = "Audio/SE/EnemyDeath",
            Position = transform.position
        });
    }
}
```

---

## 目录结构

```
Resources/
└── Audio/
    ├── BGM/
    │   ├── B_001.mp3      ← 启动 BGM (配置中指定)
    │   ├── MainMenu.mp3
    │   └── GameLevel.mp3
    │
    └── SE/
        ├── Click.mp3
        ├── Jump.mp3
        └── Attack.mp3
```

---

## 注意事项

1. **Resources 路径**：音频文件必须放在 `Resources/Audio/` 目录下
2. **对象池**：SFX 使用对象池管理，无需手动释放
3. **启动 BGM**：在 `GameSettingConfig` 中配置 `startupBGMPath`
4. **单例访问**：使用 `AudioManager.Instance` 访问
5. **空值检查**：播放前务必检查 AudioClip 是否为 null

---

## 相关文件

| 文件 | 说明 |
|------|------|
| [AudioSystem.cs](../Audio/AudioSystem.cs) | IGameSystem 包装器 |
| [AudioManager.cs](../Audio/AudioManager.cs) | 音频管理器实现 |
| [PooledAudioSource.cs](../Audio/PooledAudioSource.cs) | 池化音频源 |
| [SoundConfig.cs](../Audio/SoundConfig.cs) | 音效配置结构 |
| [GameSettingConfig.cs](../Data/Config/GameSettingConfig.cs) | 游戏配置 |
