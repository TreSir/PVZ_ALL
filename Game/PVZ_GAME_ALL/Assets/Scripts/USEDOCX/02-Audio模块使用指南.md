# Audio 音频模块使用指南

## 概述

Audio 模块负责游戏中所有音频的播放和管理，包括背景音乐（BGM）和音效（SFX）。该模块使用对象池技术优化 AudioSource 的创建和销毁，并实现了 `IGameSystem` 接口以支持自动初始化。

## 主要功能

- 🎵 背景音乐播放（支持循环、音量调节）
- 🔊 音效播放（支持3D音效、同时播放多个）
- 🏊 对象池管理（复用 AudioSource，减少 GC）
- ⚙️ 配置驱动（通过 ScriptableObject 配置）

## 配置说明

### 创建音频配置

1. 在 `Resources/Configs/` 目录下创建 `GameSettingConfig.asset`
2. 配置音频参数：

```csharp
// GameSettingConfig.asset
Audio Settings:
- Master Volume: 1.0      // 主音量 (0-1)
- BGM Volume: 0.8         // 背景音乐音量 (0-1)
- SFX Volume: 1.0         // 音效音量 (0-1)

Pool Settings:
- Audio Initial Pool Size: 10   // 初始池大小
- Audio Max Pool Size: 30        // 最大池大小
```

## 基础使用

### 获取实例

```csharp
// 通过单例获取（推荐）
AudioManager audioManager = AudioManager.Instance;

// 检查实例是否有效
if (AudioManager.HasInstance)
{
    // 使用音频管理器
}
```

### 播放背景音乐

```csharp
// 加载并播放 BGM
AudioClip bgmClip = Resources.Load<AudioClip>("Audio/BGM_MainMenu");
AudioManager.Instance.PlayBGM(bgmClip);

// 使用默认配置播放
AudioManager.Instance.PlayBGM(bgmClip, SoundConfig.BGM);

// 使用自定义配置播放
var config = new SoundConfig
{
    volume = 0.8f,
    pitch = 1.0f,
    loop = true,
    spatialBlend = 0f,
    priority = 128
};
AudioManager.Instance.PlayBGM(bgmClip, config);

// 停止 BGM
AudioManager.Instance.StopBGM();

// 暂停/恢复
AudioManager.Instance.PauseBGM();
AudioManager.Instance.ResumeBGM();

// 检查 BGM 是否在播放
bool isPlaying = AudioManager.Instance.IsBGMPlaying;
```

### 播放音效

```csharp
// 加载并播放音效
AudioClip sfxClip = Resources.Load<AudioClip>("Audio/SE_Click");
AudioManager.Instance.PlaySFX(sfxClip);

// 使用自定义配置
var config = new SoundConfig
{
    volume = 1.0f,
    pitch = 1.0f,
    loop = false,
    spatialBlend = 0f,
    priority = 128
};
AudioManager.Instance.PlaySFX(sfxClip, config);

// 播放 3D 音效（指定位置）
AudioManager.Instance.PlaySFXAtPosition(sfxClip, new Vector3(10, 0, 5));

// 停止所有 SFX
AudioManager.Instance.StopAllSFX();

// 停止特定音效
// 需要保存 PlaySFX 的返回值
AudioSource playingSource = AudioManager.Instance.PlaySFX(sfxClip);
AudioManager.Instance.StopSFX(playingSource);
```

### 播放 3D 音效

```csharp
// 3D 音效会自动根据位置调整音量和平移
// spatialBlend 默认为 0（2D音效），设置为 1 为完全 3D

var config = new SoundConfig
{
    volume = 1.0f,
    pitch = 1.0f,
    loop = false,
    spatialBlend = 1f,  // 完全 3D
    priority = 128
};
AudioManager.Instance.PlaySFXAtPosition(sfxClip, transform.position, config);
```

## 音量控制

### 单独控制音量

```csharp
// 设置各个音量（值范围 0-1）
AudioManager.Instance.MasterVolume = 0.8f;  // 主音量
AudioManager.Instance.BGMVolume = 0.6f;     // BGM 音量
AudioManager.Instance.SFXVolume = 1.0f;     // SFX 音量

// 获取当前音量
float master = AudioManager.Instance.MasterVolume;
float bgm = AudioManager.Instance.BGMVolume;
float sfx = AudioManager.Instance.SFXVolume;
```

### 批量设置音量

```csharp
AudioManager.Instance.SetVolumeSettings(
    masterVolume: 0.8f,
    bgmVolume: 0.6f,
    sfxVolume: 1.0f
);
```

### 淡入淡出效果

```csharp
using System.Collections;

public class AudioFadeExample : MonoBehaviour
{
    public AudioClip newBGM;

    public IEnumerator FadeOutBGM(float duration)
    {
        float startVolume = AudioManager.Instance.BGMVolume;
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            AudioManager.Instance.BGMVolume = Mathf.Lerp(startVolume, 0f, timer / duration);
            yield return null;
        }

        AudioManager.Instance.StopBGM();
        AudioManager.Instance.BGMVolume = startVolume;
    }

    public IEnumerator FadeInBGM(float duration)
    {
        AudioManager.Instance.PlayBGM(newBGM);
        float targetVolume = AudioManager.Instance.BGMVolume;
        AudioManager.Instance.BGMVolume = 0f;

        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            AudioManager.Instance.BGMVolume = Mathf.Lerp(0f, targetVolume, timer / duration);
            yield return null;
        }

        AudioManager.Instance.BGMVolume = targetVolume;
    }
}
```

## 全局控制

```csharp
// 暂停所有音频
AudioManager.Instance.PauseAll();

// 恢复所有音频
AudioManager.Instance.ResumeAll();

// 停止所有音频
AudioManager.Instance.StopAll();
```

## SoundConfig 配置说明

```csharp
public struct SoundConfig
{
    /// <summary>音量 (0-1)</summary>
    public float volume;

    /// <summary>音调 (0.5-2.0, 1.0 为正常)</summary>
    public float pitch;

    /// <summary>是否循环</summary>
    public bool loop;

    /// <summary>空间混合 (0 = 2D, 1 = 3D)</summary>
    public float spatialBlend;

    /// <summary>优先级 (0 = 最高, 256 = 默认)</summary>
    public int priority;
}
```

### 默认配置

```csharp
// BGM 默认配置
SoundConfig.BGM = new SoundConfig
{
    volume = 1.0f,
    pitch = 1.0f,
    loop = true,
    spatialBlend = 0f,
    priority = 128
};

// SFX 默认配置
SoundConfig.SFX = new SoundConfig
{
    volume = 1.0f,
    pitch = 1.0f,
    loop = false,
    spatialBlend = 0f,
    priority = 128
};
```

## 与 Core 模块集成

AudioManager 实现了 `IGameSystem` 接口，会被 GameInitializer 自动发现和初始化。

```csharp
// AudioManager 的初始化是自动的，无需手动调用
// 但如果需要手动初始化：

AudioManager.Instance.Initialize();

// 初始化会：
// 1. 加载 GameSettingConfig
// 2. 设置音量
// 3. 初始化对象池
```

## 注意事项

1. **Resources 路径**：音频文件必须放在 `Resources/Audio/` 目录下
2. **对象池**：SFX 使用对象池管理，无需手动释放
3. **场景切换**：切换场景时池会自动释放（可在配置中关闭）
4. **Null 检查**：播放前务必检查 AudioClip 是否为 null
5. **事件系统**：建议结合 EventBus 使用，在特定事件触发音效

## 完整示例

```csharp
using UnityEngine;
using Audio;

public class AudioExample : MonoBehaviour
{
    private void Start()
    {
        // 播放背景音乐
        var bgm = Resources.Load<AudioClip>("Audio/BGM_Game");
        if (bgm != null)
        {
            AudioManager.Instance.PlayBGM(bgm);
        }
    }

    private void Update()
    {
        // 按空格播放音效
        if (Input.GetKeyDown(KeyCode.Space))
        {
            PlayClickSound();
        }

        // 按 ESC 暂停/恢复
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (Time.timeScale == 0)
                AudioManager.Instance.ResumeAll();
            else
                AudioManager.Instance.PauseAll();
        }
    }

    private void PlayClickSound()
    {
        var clip = Resources.Load<AudioClip>("Audio/SE_Click");
        if (clip != null)
        {
            AudioManager.Instance.PlaySFX(clip);
        }
    }
}
```

## 相关文件

- [AudioManager.cs](../Audio/AudioManager.cs) - 音频管理器实现
- [PooledAudioSource.cs](../Audio/PooledAudioSource.cs) - 池化音频源
- [SoundConfig.cs](../Audio/SoundConfig.cs) - 音效配置
- [GameSettingConfig.cs](../Data/Config/GameSettingConfig.cs) - 游戏配置
