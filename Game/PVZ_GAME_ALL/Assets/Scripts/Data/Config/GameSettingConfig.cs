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
}
