using System;
using UnityEngine;

namespace Audio
{
    /// <summary>
    /// 音效配置类,播放音效时,代码调节当前音效的音量、音调、循环设置等参数.
    /// </summary>
    [Serializable]
    public class SoundConfig
    {
        [Tooltip("音量 (0-1)")]
        [Range(0f, 1f)]
        public float volume = 1f;

        [Tooltip("音调 (-3 到 3)")]
        [Range(-3f, 3f)]
        public float pitch = 1f;

        [Tooltip("是否循环")]
        public bool loop = false;

        [Tooltip("空间混合 (0=2D, 1=3D)")]
        [Range(0f, 1f)]
        public float spatialBlend = 0f;

        [Tooltip("优先级")]
        [Range(0, 256)]
        public int priority = 128;

        public SoundConfig() { }

        public SoundConfig(float volume, float pitch = 1f, bool loop = false)
        {
            this.volume = volume;
            this.pitch = pitch;
            this.loop = loop;
        }

        public static SoundConfig Default => new SoundConfig();

        public static SoundConfig BGM => new SoundConfig(1f, 1f, true);

        public static SoundConfig SFX => new SoundConfig(1f, 1f, false);
    }
}
