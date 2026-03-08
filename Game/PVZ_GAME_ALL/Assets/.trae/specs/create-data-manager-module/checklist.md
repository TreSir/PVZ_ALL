# Checklist

## 基础结构
- [x] DataManager 目录已创建
- [x] DataManager.asmdef 程序集定义文件已创建并正确配置引用（GameBase、Config）

## DataManager 核心
- [x] DataManager 类已实现 IGameSystem 接口
- [x] Priority 设置为 50
- [x] 单例模式正确实现
- [x] Initialize() 正确加载配置和初始化缓存
- [x] Shutdown() 正确清空缓存

## 数据加载功能
- [x] Load<T>(path) 方法正确从 Resources 加载资源
- [x] LoadConfig<T>() 方法正确加载 ScriptableObject 配置
- [x] LoadJson<T>(path) 方法正确加载 JSON 文件并反序列化
- [x] 加载的数据自动缓存

## 数据管理功能
- [x] Register<T>(key, data) 方法正确注册数据到缓存
- [x] Get<T>(key) 方法正确从缓存获取数据
- [x] Unregister(key) 方法正确移除缓存数据
- [x] Clear() 方法正确清空所有缓存

## 序列化功能
- [x] Serialize<T>(data) 方法使用 Newtonsoft JSON 正确序列化
- [x] Deserialize<T>(json) 方法使用 Newtonsoft JSON 正确反序列化
- [x] SaveToFile<T>(data, fileName) 方法正确保存到持久化目录
- [x] LoadFromFile<T>(fileName) 方法正确从文件加载
- [x] FileExists(fileName) 方法正确检查文件存在
- [x] DeleteFile(fileName) 方法正确删除文件

## 配置扩展
- [x] GameSettingConfig 已添加 DataSettings 类
- [x] GameSettingConfig 主类已添加 data 字段

## 集成验证
- [x] GameInitializer 能正确发现 DataManager（实现 IGameSystem 接口）
- [x] DataManager 在其他系统之前初始化（Priority: 50）
- [x] 数据加载功能正常工作
- [x] 序列化和反序列化功能正常工作
- [x] 文件保存和加载功能正常工作
