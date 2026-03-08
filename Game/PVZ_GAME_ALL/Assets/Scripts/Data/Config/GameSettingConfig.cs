using UnityEngine;

namespace Config
{
    [CreateAssetMenu(fileName = "GameSettingConfig", menuName = "Config/GameSettingConfig")]
    public class GameSettingConfig : ScriptableObject
    {
        [Header("Audio Settings")]
        public AudioSettings audio = new AudioSettings();

        [Header("Pool Settings")]
        public PoolSettings pool = new PoolSettings();

        [Header("UI Settings")]
        public UISettings ui = new UISettings();

        [Header("Data Settings")]
        public DataSettings data = new DataSettings();
    }

    [System.Serializable]
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

        [Tooltip("游戏启动时播放的背景音乐路径（Resources 下的相对路径）")]
        public string startupBGMPath = "Audio/BGM/B_001";
    }

    [System.Serializable]
    public class PoolSettings
    {
        [Tooltip("AudioSource 池初始大小")]
        [Min(1)]
        public int audioInitialPoolSize = 10;

        [Tooltip("AudioSource 池最大大小")]
        [Min(1)]
        public int audioMaxPoolSize = 30;
    }

    [System.Serializable]
    public class UISettings
    {
        [Tooltip("游戏启动时打开的第一个 UI 面板名称")]
        public string startupPanelName = "MainMenuPanel";

        [Tooltip("是否启用 UI 面板对象池")]
        public bool enablePanelPool = true;

        [Tooltip("UI 面板池初始大小")]
        [Min(1)]
        public int panelPoolInitialSize = 3;
    }

    [System.Serializable]
    public class DataSettings
    {
        [Tooltip("是否启用配置数据缓存")]
        public bool enableCache = true;

        [Tooltip("持久化数据存储目录名")]
        public string persistentFolderName = "GameData";

        [Tooltip("自动保存间隔（秒），0表示禁用自动保存")]
        public float autoSaveInterval = 60f;
    }
}
