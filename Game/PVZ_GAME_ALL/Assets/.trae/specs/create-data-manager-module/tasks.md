# Tasks

- [x] Task 1: 创建 DataManager 模块基础结构
  - [x] SubTask 1.1: 创建 Scripts/DataManager 目录
  - [x] SubTask 1.2: 创建 DataManager.asmdef 程序集定义文件，引用 GameBase、Config

- [x] Task 2: 实现 DataManager 核心类
  - [x] SubTask 2.1: 创建 DataManager.cs 主类，实现 IGameSystem 接口，Priority = 50
  - [x] SubTask 2.2: 实现单例模式 Instance 属性
  - [x] SubTask 2.3: 实现 Initialize() 方法：加载配置、初始化缓存
  - [x] SubTask 2.4: 实现 Shutdown() 方法：清空缓存

- [x] Task 3: 实现数据加载功能
  - [x] SubTask 3.1: 实现 Load<T>(path) 方法，从 Resources 加载资源
  - [x] SubTask 3.2: 实现 LoadConfig<T>() 方法，加载 ScriptableObject 配置
  - [x] SubTask 3.3: 实现 LoadJson<T>(path) 方法，加载 JSON 文件并反序列化

- [x] Task 4: 实现数据管理功能
  - [x] SubTask 4.1: 实现 Register<T>(key, data) 方法，注册数据到缓存
  - [x] SubTask 4.2: 实现 Get<T>(key) 方法，从缓存获取数据
  - [x] SubTask 4.3: 实现 Unregister(key) 方法，移除缓存数据
  - [x] SubTask 4.4: 实现 Clear() 方法，清空所有缓存

- [x] Task 5: 实现序列化功能（Newtonsoft JSON）
  - [x] SubTask 5.1: 实现 Serialize<T>(data) 方法，序列化对象为 JSON 字符串
  - [x] SubTask 5.2: 实现 Deserialize<T>(json) 方法，反序列化 JSON 字符串为对象
  - [x] SubTask 5.3: 实现 SaveToFile<T>(data, fileName) 方法，序列化并保存到文件
  - [x] SubTask 5.4: 实现 LoadFromFile<T>(fileName) 方法，从文件加载并反序列化
  - [x] SubTask 5.5: 实现 FileExists(fileName) 方法，检查文件是否存在
  - [x] SubTask 5.6: 实现 DeleteFile(fileName) 方法，删除文件

- [x] Task 6: 扩展 GameSettingConfig 配置
  - [x] SubTask 6.1: 在 GameSettingConfig.cs 中添加 DataSettings 类定义
  - [x] SubTask 6.2: 在 GameSettingConfig 主类中添加 data 字段

- [x] Task 7: 测试和验证
  - [x] SubTask 7.1: 验证 DataManager 被 GameInitializer 正确发现和初始化（Priority: 50）
  - [x] SubTask 7.2: 测试数据加载功能
  - [x] SubTask 7.3: 测试序列化和反序列化功能
  - [x] SubTask 7.4: 测试文件保存和加载功能

# Task Dependencies
- [Task 2] depends on [Task 1]
- [Task 3] depends on [Task 2]
- [Task 4] depends on [Task 2]
- [Task 5] depends on [Task 2]
- [Task 7] depends on [Task 3, Task 4, Task 5, Task 6]
