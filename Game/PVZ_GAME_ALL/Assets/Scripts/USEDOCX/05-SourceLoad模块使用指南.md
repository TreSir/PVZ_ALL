# SourceLoad 资源加载模块使用指南

## 概述

SourceLoad 模块提供统一的资源加载接口，支持同步和异步加载。该模块采用策略模式设计，通过接口抽象可以轻松扩展新的加载方式（如 Addressables、Web请求等）。

## 核心组件

### IResourceLoader - 资源加载器接口

```csharp
public interface IResourceLoader
{
    // 同步加载
    T Load<T>(string path) where T : UnityEngine.Object;

    // 异步加载
    void LoadAsync<T>(string path, Action<T> onComplete) where T : UnityEngine.Object;

    // 卸载资源
    void Unload(UnityEngine.Object asset);
}
```

### ResourceManager - 资源管理器

静态类，提供统一的资源访问入口：

```csharp
public static class ResourceManager
{
    // 初始化（自动调用）
    public static void Initialize();

    // 同步加载
    public static T Load<T>(string path) where T : UnityEngine.Object;

    // 异步加载
    public static void LoadAsync<T>(string path, Action<T> onComplete) where T : UnityEngine.Object;

    // 卸载
    public static void Unload(UnityEngine.Object asset);
}
```

## 基本使用

### 同步加载

```csharp
using SourceLoad;

// 加载 Prefab
GameObject prefab = ResourceManager.Load<GameObject>("Prefabs/Player");
GameObject player = Instantiate(prefab);

// 加载音频
AudioClip bgm = ResourceManager.Load<AudioClip>("Audio/BGM_Main");
AudioClip sfx = ResourceManager.Load<AudioClip>("Audio/SE_Click");

// 加载 Sprite
Sprite sprite = ResourceManager.Load<Sprite>("UI/Icons/Icon01");

// 加载 TextAsset
TextAsset json = ResourceManager.Load<TextAsset>("Data/Config/GameConfig");

// 加载 ScriptableObject
GameSettingConfig config = ResourceManager.Load<GameSettingConfig>("Configs/GameSettingConfig");
```

### 异步加载

```csharp
using SourceLoad;

// 异步加载 Prefab
ResourceManager.LoadAsync<GameObject>("Prefabs/Enemy", (prefab) =>
{
    if (prefab != null)
    {
        GameObject enemy = Instantiate(prefab);
    }
});

// 异步加载音频
ResourceManager.LoadAsync<AudioClip>("Audio/BGM_Game", (clip) =>
{
    if (clip != null)
    {
        AudioManager.Instance.PlayBGM(clip);
    }
});

// 异步加载多个资源
StartCoroutine(LoadMultipleAssets());

private IEnumerator LoadMultipleAssets()
{
    var operations = new List<ResourceRequest>
    {
        Resources.LoadAsync<GameObject>("Prefabs/Player"),
        Resources.LoadAsync<AudioClip>("Audio/BGM_Main"),
        Resources.LoadAsync<Sprite>("UI/Icons/PlayerIcon")
    };

    foreach (var op in operations)
    {
        yield return op;
        Debug.Log($"Loaded: {op.asset.name}");
    }
}
```

### 卸载资源

```csharp
// 加载资源
Sprite sprite = ResourceManager.Load<Sprite>("UI/BG");

// 卸载单个资源
ResourceManager.Unload(sprite);

// 卸载所有未使用的资源
Resources.UnloadUnusedAssets();

// 完全清理内存
Resources.UnloadUnusedAssets();
GC.Collect();
```

## Resources 目录结构

资源文件需要放在 `Resources` 目录下：

```
Assets/
└── Resources/
    ├── Audio/
    │   ├── BGM/
    │   │   ├── MainMenu.ogg
    │   │   └── Game.ogg
    │   └── SE/
    │       ├── Click.ogg
    │       └── Jump.ogg
    │
    ├── Prefabs/
    │   ├── Characters/
    │   │   ├── Player.prefab
    │   │   └── Enemy.prefab
    │   └── Effects/
    │       └── Explosion.prefab
    │
    ├── UI/
    │   ├── Icons/
    │   │   └── Icon01.png
    │   └── Panels/
    │       └── MainMenu.prefab
    │
    ├── Data/
    │   └── Config/
    │       └── GameConfig.json
    │
    └── Configs/
        └── GameSettingConfig.asset
```

## 扩展加载器

### 自定义加载器

可以实现 IResourceLoader 接口创建自定义加载器：

```csharp
using UnityEngine;
using SourceLoad;

public class CustomLoader : IResourceLoader
{
    public T Load<T>(string path) where T : UnityEngine.Object
    {
        // 自定义加载逻辑
        // 例如：从 AssetBundle 加载
        return LoadFromAssetBundle<T>(path);
    }

    public void LoadAsync<T>(string path, Action<T> onComplete) where T : UnityEngine.Object
    {
        // 自定义异步加载逻辑
    }

    public void Unload(UnityEngine.Object asset)
    {
        // 自定义卸载逻辑
    }

    private T LoadFromAssetBundle<T>(string path) where T : UnityEngine.Object
    {
        // AssetBundle 加载实现
    }
}
```

### 切换加载器

```csharp
// 设置自定义加载器
ResourceManager.SetLoader(new CustomLoader());

// 切换回默认加载器
ResourceManager.SetLoader(new ResourcesLoader());
```

## 最佳实践

### 1. 使用泛型指定类型

```csharp
// ✅ 好：明确指定类型
GameObject player = ResourceManager.Load<GameObject>("Prefabs/Player");
AudioClip bgm = ResourceManager.Load<AudioClip>("Audio/BGM");

// ❌ 不好：使用 Object 类型
Object obj = ResourceManager.Load<Object>("Prefabs/Player");
```

### 2. 路径不带扩展名

```csharp
// ✅ 正确：不需要扩展名
Sprite sprite = ResourceManager.Load<Sprite>("UI/Icons/Icon01");

// ❌ 错误：不需要 .png 后缀
Sprite sprite = ResourceManager.Load<Sprite>("UI/Icons/Icon01.png");
```

### 3. 合理使用异步加载

```csharp
// ✅ 好：大资源使用异步
ResourceManager.LoadAsync<GameObject>("Prefabs/HeavyCharacter", (prefab) => { ... });

// ✅ 好：小资源可以使用同步
AudioClip click = ResourceManager.Load<AudioClip>("Audio/SE_Click");
```

### 4. 资源缓存

```csharp
public class ResourceCache
{
    private static readonly Dictionary<string, UnityEngine.Object> _cache = new Dictionary<string, UnityEngine.Object>();

    public static T Load<T>(string path) where T : UnityEngine.Object
    {
        if (_cache.TryGetValue(path, out var cached))
        {
            return cached as T;
        }

        var asset = ResourceManager.Load<T>(path);
        if (asset != null)
        {
            _cache[path] = asset;
        }
        return asset;
    }

    public static void Clear()
    {
        _cache.Clear();
    }
}
```

### 5. 预加载常用资源

```csharp
public class ResourcePreloader : MonoBehaviour
{
    private void Start()
    {
        // 预加载常用资源
        PreloadCommonResources();
    }

    private void PreloadCommonResources()
    {
        // UI 资源
        ResourceManager.Load<Sprite>("UI/Icons/Icon01");
        ResourceManager.Load<Sprite>("UI/Icons/Icon02");

        // 音频
        ResourceManager.Load<AudioClip>("Audio/SE_Click");
        ResourceManager.Load<AudioClip>("Audio/SE_Jump");

        // 预制体
        ResourceManager.Load<GameObject>("Prefabs/Player");
    }
}
```

## 配置加载

### 加载 ScriptableObject 配置

```csharp
// 加载游戏配置
GameSettingConfig gameConfig = ResourceManager.Load<GameSettingConfig>("Configs/GameSettingConfig");

if (gameConfig != null)
{
    // 使用配置
    float masterVolume = gameConfig.audio.masterVolume;
    float bgmVolume = gameConfig.audio.bgmVolume;
    float sfxVolume = gameConfig.audio.sfxVolume;

    int initialPoolSize = gameConfig.pool.audioInitialPoolSize;
    int maxPoolSize = gameConfig.pool.audioMaxPoolSize;
}
```

### 加载 JSON 配置

```csharp
// 加载 JSON 文件
TextAsset jsonFile = ResourceManager.Load<TextAsset>("Data/Config/GameConfig");
if (jsonFile != null)
{
    GameConfigData data = JsonUtility.FromJson<GameConfigData>(jsonFile.text);
    Debug.Log($"Loaded config: {data.gameName}");
}

[System.Serializable]
public class GameConfigData
{
    public string gameName;
    public int maxPlayers;
    public float gameTime;
}
```

## 性能优化

### 1. 资源分块加载

```csharp
private IEnumerator LoadAssetsInChunks(string[] paths)
{
    int chunkSize = 5;
    for (int i = 0; i < paths.Length; i += chunkSize)
    {
        var chunk = paths.Skip(i).Take(chunkSize);
        foreach (var path in chunk)
        {
            ResourceManager.LoadAsync<GameObject>(path, (prefab) => { });
        }
        yield return new WaitForEndOfFrame(); // 每帧加载一部分
    }
}
```

### 2. 使用对象池减少加载

```csharp
public class PrefabLoader
{
    private Dictionary<string, GameObject> _prefabCache = new Dictionary<string, GameObject>();

    public GameObject GetPrefab(string path)
    {
        if (_prefabCache.TryGetValue(path, out var cached))
        {
            return cached;
        }

        var prefab = ResourceManager.Load<GameObject>(path);
        if (prefab != null)
        {
            _prefabCache[path] = prefab;
        }
        return prefab;
    }
}
```

### 3. 资源按需加载

```csharp
// 按需加载，而不是一次性加载所有
public class LazyResourceLoader<T> where T : UnityEngine.Object
{
    private T _resource;
    private string _path;

    public LazyResourceLoader(string path)
    {
        _path = path;
    }

    public T Value
    {
        get
        {
            if (_resource == null)
            {
                _resource = ResourceManager.Load<T>(_path);
            }
            return _resource;
        }
    }
}

// 使用
private LazyResourceLoader<GameObject> _playerPrefab = new LazyResourceLoader<GameObject>("Prefabs/Player");

private void SpawnPlayer()
{
    // 第一次访问时才加载
    Instantiate(_playerPrefab.Value);
}
```

## 注意事项

1. **Resources 目录**：所有需要动态加载的资源必须放在 `Resources` 文件夹下
2. **路径格式**：使用相对路径，不带文件扩展名
3. **空值检查**：加载后务必检查是否为 null
4. **内存管理**：不用的资源及时卸载，避免内存泄漏
5. **异步加载**：大资源建议使用异步加载，避免卡顿

## 常见问题

### Q: 资源加载失败怎么办？

```csharp
var prefab = ResourceManager.Load<GameObject>("Prefabs/Player");
if (prefab == null)
{
    Debug.LogError("Failed to load prefab!");
    return;
}
```

### Q: 如何加载同名资源？

```csharp
// Resources.Load 会返回第一个找到的资源
// 如需加载多个同名资源，使用 Resources.LoadAll
var sprites = Resources.LoadAll<Sprite>("UI/Icons");
foreach (var sprite in sprites)
{
    Debug.Log(sprite.name);
}
```

### Q: 如何加载子文件夹中的资源？

```csharp
// 使用完整路径
var prefab = ResourceManager.Load<GameObject>("Prefabs/Characters/Player");
var audio = ResourceManager.Load<AudioClip>("Audio/BGM/Menu/Main");
```

## 相关文件

- [ResourceManager.cs](../SourceLoad/ResourceManager.cs) - 资源管理器
- [IResourceLoader.cs](../SourceLoad/IResourceLoader.cs) - 加载器接口
- [ResourcesLoader.cs](../SourceLoad/ResourcesLoader.cs) - 默认加载器实现
