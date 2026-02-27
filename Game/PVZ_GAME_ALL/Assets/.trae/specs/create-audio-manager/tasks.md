# Tasks

- [x] Task 1: 创建基础文件结构和配置类
  - [x] SubTask 1.1: 创建 Scripts/Audio 文件夹结构
  - [x] SubTask 1.2: 创建 SoundConfig.cs 音频配置类
  - [x] SubTask 1.3: 创建 Audio.asmdef 程序集定义文件

- [x] Task 2: 实现 AudioManager 核心功能
  - [x] SubTask 2.1: 创建 AudioManager.cs 单例管理器
  - [x] SubTask 2.2: 实现 BGM 播放功能（支持循环、音量控制）
  - [x] SubTask 2.3: 实现 SFX 播放功能（使用 AudioSource 池）

- [x] Task 3: 实现音量控制系统
  - [x] SubTask 3.1: 实现主音量控制
  - [x] SubTask 3.2: 实现 BGM/SFX 分组音量控制

- [x] Task 4: 实现 AudioSource 池化
  - [x] SubTask 4.1: 创建 AudioSource 池，支持多个音效同时播放
  - [x] SubTask 4.2: 实现播放完成自动回收

# Task Dependencies
- [Task 2] depends on [Task 1]
- [Task 3] depends on [Task 2]
- [Task 4] depends on [Task 2]
