# 音频管理器 Spec

## Why
游戏需要一个统一的音频管理器来处理所有音频播放，让游戏对象可以通过管理器播放音乐和音效，避免每个游戏对象单独管理 AudioSource。

## What Changes
- 在 `Scripts/Audio` 文件夹下创建音频管理模块
- 实现 AudioManager 单例管理器
- 实现 SoundConfig 配置类
- 实现 AudioSource 池化，支持多个音效同时播放

## Impact
- Affected specs: 无，新增模块
- Affected code: 新增 Scripts/Audio 文件夹

## ADDED Requirements

### Requirement: AudioManager Core
The system SHALL provide a singleton AudioManager that manages all audio playback.

#### Scenario: Play Sound Effect
- **WHEN** 游戏对象调用 AudioManager.PlaySFX(clip)
- **THEN** 系统使用池化的 AudioSource 播放音效

#### Scenario: Play Background Music
- **WHEN** 游戏对象调用 AudioManager.PlayBGM(clip)
- **THEN** 系统播放背景音乐，支持循环播放

#### Scenario: Stop Audio
- **WHEN** 游戏对象调用 AudioManager.StopSFX() 或 AudioManager.StopBGM()
- **THEN** 系统停止对应的音频播放

### Requirement: Volume Control
The system SHALL support volume control for different audio types.

#### Scenario: Set Master Volume
- **WHEN** 设置 AudioManager.MasterVolume
- **THEN** 所有音频音量随之改变

#### Scenario: Set BGM/SFX Volume
- **WHEN** 分别设置 BGMVolume 或 SFXVolume
- **THEN** 对应类型音频音量改变

### Requirement: Sound Configuration
The system SHALL provide SoundConfig class to define audio properties.

#### Scenario: Use Sound Config
- **WHEN** 播放音频时传入 SoundConfig
- **THEN** 音频按照配置的音量、音调、循环等属性播放

### Requirement: AudioSource Pooling
The system SHALL pool AudioSource components for efficient playback.

#### Scenario: Reuse AudioSource
- **WHEN** 音效播放完成
- **THEN** AudioSource 返回池中等待复用
