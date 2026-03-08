# SourceLoad 资源加载模块使用指南

## 概述

SourceLoad 提供统一的资源加载接口，支持同步和异步加载，采用策略模式设计。

---

## 基本使用

### 同步加载

```csharp
using SourceLoad;

GameObject prefab = ResourceManager.Load<GameObject>("Prefabs/Player");
AudioClip clip = ResourceManager.Load<AudioClip>("Audio/BGM_Main");
Sprite sprite = ResourceManager.Load<Sprite>("UI/Icons/Icon01");
```

### 异步加载

```csharp
ResourceManager.LoadAsync<GameObject>("Prefabs/Enemy", (prefab) =>
{
    if (prefab != null)
    {
        Instantiate(prefab);
    }
});
```

### 卸载资源

```csharp
ResourceManager.Unload(sprite);
Resources.UnloadUnusedAssets();
```

---

## 目录结构

```
Resources/
├── Audio/
├── Prefabs/
├── UI/
├── Data/
└── Configs/
```

---

## 扩展加载器

```csharp
public class CustomLoader : IResourceLoader
{
    public T Load<T>(string path) where T : Object { }
    public void LoadAsync<T>(string path, Action<T> onComplete) where T : Object { }
    public void Unload(Object asset) { }
}

ResourceManager.SetLoader(new CustomLoader());
```

---

## API 速查

| 方法 | 说明 |
|------|------|
| `Load<T>(path)` | 同步加载 |
| `LoadAsync<T>(path, callback)` | 异步加载 |
| `Unload(asset)` | 卸载资源 |
| `SetLoader(loader)` | 设置加载器 |

---

## 注意事项

- 当前仅支持 Resources 加载
- 可扩展实现 Addressables 加载器
