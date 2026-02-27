using System.Collections.Generic;
using UnityEngine;
using Config;
using SourceLoad;

namespace Audio
{
    /// <summary>
    /// 音频管理器类,负责播放背景音乐和音效,以及管理音频源的池化.
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        private static AudioManager _instance;
        private static readonly object _lock = new object();
        private static bool _applicationIsQuitting;

        public static AudioManager Instance
        {
            get
            {
                if (_applicationIsQuitting)
                {
                    Debug.LogWarning("[AudioManager] Application is quitting, returning null.");
                    return null;
                }

                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = FindObjectOfType<AudioManager>();

                        if (_instance == null)
                        {
                            var go = new GameObject("[AudioManager]");
                            _instance = go.AddComponent<AudioManager>();
                            DontDestroyOnLoad(go);
                        }
                    }

                    return _instance;
                }
            }
        }

        public static bool HasInstance => _instance != null && !_applicationIsQuitting;

        [Header("Volume Settings")]
        [SerializeField]
        [Range(0f, 1f)]
        private float _masterVolume = 1f;

        [SerializeField]
        [Range(0f, 1f)]
        private float _bgmVolume = 1f;

        [SerializeField]
        [Range(0f, 1f)]
        private float _sfxVolume = 1f;

        [Header("Pool Settings")]
        [SerializeField]
        private int _initialPoolSize = 10;

        [SerializeField]
        private int _maxPoolSize = 30;

        private AudioSource _bgmSource;
        private readonly List<AudioSource> _sfxPool = new List<AudioSource>();
        private readonly List<AudioSource> _activeSfxSources = new List<AudioSource>();
        private Transform _poolRoot;

        public float MasterVolume
        {
            get => _masterVolume;
            set
            {
                _masterVolume = Mathf.Clamp01(value);
                UpdateAllVolumes();
            }
        }

        public float BGMVolume
        {
            get => _bgmVolume;
            set
            {
                _bgmVolume = Mathf.Clamp01(value);
                if (_bgmSource != null)
                {
                    _bgmSource.volume = _masterVolume * _bgmVolume;
                }
            }
        }

        public float SFXVolume
        {
            get => _sfxVolume;
            set
            {
                _sfxVolume = Mathf.Clamp01(value);
            }
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeBGMSource();
            InitializeSFXPool();
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _applicationIsQuitting = true;
            }
        }

        private void OnApplicationQuit()
        {
            _applicationIsQuitting = true;
        }

        private void Update()
        {
            RecycleFinishedSFX();
        }

        private void InitializeBGMSource()
        {
            _bgmSource = gameObject.AddComponent<AudioSource>();
            _bgmSource.playOnAwake = false;
            _bgmSource.loop = true;
            _bgmSource.volume = _masterVolume * _bgmVolume;
        }

        private void InitializeSFXPool()
        {
            _poolRoot = new GameObject("[SFX_Pool]").transform;
            _poolRoot.SetParent(transform);

            for (int i = 0; i < _initialPoolSize; i++)
            {
                CreateNewSFXSource();
            }
        }

        private AudioSource CreateNewSFXSource()
        {
            var source = _poolRoot.gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = false;
            _sfxPool.Add(source);
            return source;
        }

        private void UpdateAllVolumes()
        {
            if (_bgmSource != null)
            {
                _bgmSource.volume = _masterVolume * _bgmVolume;
            }
        }

        private void RecycleFinishedSFX()
        {
            for (int i = _activeSfxSources.Count - 1; i >= 0; i--)
            {
                var source = _activeSfxSources[i];
                if (source == null || !source.isPlaying)
                {
                    if (source != null)
                    {
                        source.clip = null;
                        source.transform.SetParent(_poolRoot);
                        _sfxPool.Add(source);
                    }
                    _activeSfxSources.RemoveAt(i);
                }
            }
        }

        private AudioSource GetAvailableSFXSource()
        {
            if (_sfxPool.Count > 0)
            {
                var source = _sfxPool[_sfxPool.Count - 1];
                _sfxPool.RemoveAt(_sfxPool.Count - 1);
                return source;
            }

            if (_activeSfxSources.Count + _sfxPool.Count < _maxPoolSize)
            {
                return CreateNewSFXSource();
            }

            if (_activeSfxSources.Count > 0)
            {
                var oldest = _activeSfxSources[0];
                oldest.Stop();
                oldest.clip = null;
                _activeSfxSources.RemoveAt(0);
                return oldest;
            }

            return null;
        }

        #region BGM Methods

        public void PlayBGM(AudioClip clip, SoundConfig config = null)
        {
            if (clip == null) return;

            config = config ?? SoundConfig.BGM;

            _bgmSource.clip = clip;
            _bgmSource.volume = _masterVolume * _bgmVolume * config.volume;
            _bgmSource.pitch = config.pitch;
            _bgmSource.loop = config.loop;
            _bgmSource.Play();
        }

        public void StopBGM()
        {
            if (_bgmSource != null && _bgmSource.isPlaying)
            {
                _bgmSource.Stop();
            }
        }

        public void PauseBGM()
        {
            if (_bgmSource != null && _bgmSource.isPlaying)
            {
                _bgmSource.Pause();
            }
        }

        public void ResumeBGM()
        {
            if (_bgmSource != null && !_bgmSource.isPlaying)
            {
                _bgmSource.UnPause();
            }
        }

        public bool IsBGMPlaying => _bgmSource != null && _bgmSource.isPlaying;

        #endregion

        #region SFX Methods

        public AudioSource PlaySFX(AudioClip clip, SoundConfig config = null)
        {
            if (clip == null) return null;

            config = config ?? SoundConfig.SFX;

            var source = GetAvailableSFXSource();
            if (source == null) return null;

            source.clip = clip;
            source.volume = _masterVolume * _sfxVolume * config.volume;
            source.pitch = config.pitch;
            source.loop = config.loop;
            source.spatialBlend = config.spatialBlend;
            source.priority = config.priority;
            source.Play();

            _activeSfxSources.Add(source);

            return source;
        }

        public AudioSource PlaySFXAtPosition(AudioClip clip, Vector3 position, SoundConfig config = null)
        {
            if (clip == null) return null;

            config = config ?? SoundConfig.SFX;
            config.spatialBlend = 1f;

            var source = GetAvailableSFXSource();
            if (source == null) return null;

            source.transform.position = position;
            source.clip = clip;
            source.volume = _masterVolume * _sfxVolume * config.volume;
            source.pitch = config.pitch;
            source.loop = config.loop;
            source.spatialBlend = config.spatialBlend;
            source.priority = config.priority;
            source.Play();

            _activeSfxSources.Add(source);

            return source;
        }

        public void StopAllSFX()
        {
            foreach (var source in _activeSfxSources)
            {
                if (source != null)
                {
                    source.Stop();
                    source.clip = null;
                    source.transform.SetParent(_poolRoot);
                    _sfxPool.Add(source);
                }
            }
            _activeSfxSources.Clear();
        }

        public void StopSFX(AudioSource source)
        {
            if (source == null) return;

            if (_activeSfxSources.Contains(source))
            {
                source.Stop();
                source.clip = null;
                source.transform.SetParent(_poolRoot);
                _activeSfxSources.Remove(source);
                _sfxPool.Add(source);
            }
        }

        #endregion

        #region Utility

        public void StopAll()
        {
            StopBGM();
            StopAllSFX();
        }

        public void PauseAll()
        {
            PauseBGM();
            foreach (var source in _activeSfxSources)
            {
                if (source != null && source.isPlaying)
                {
                    source.Pause();
                }
            }
        }

        public void ResumeAll()
        {
            ResumeBGM();
            foreach (var source in _activeSfxSources)
            {
                if (source != null && !source.isPlaying)
                {
                    source.UnPause();
                }
            }
        }

        #endregion

        #region Settings

        private const string ConfigPath = "Configs/GameSettingConfig";

        public void SetVolumeSettings(float masterVolume, float bgmVolume, float sfxVolume)
        {
            _masterVolume = Mathf.Clamp01(masterVolume);
            _bgmVolume = Mathf.Clamp01(bgmVolume);
            _sfxVolume = Mathf.Clamp01(sfxVolume);
            UpdateAllVolumes();
        }

        /// <summary>
        /// 初始化音频管理器
        /// </summary>  
        public void Initialize()
        {
            var config = ResourceManager.Load<GameSettingConfig>(ConfigPath);
            if (config == null)
            {
                Debug.LogWarning($"[AudioManager] Config not found at Resources/{ConfigPath}");
                return;
            }

            _masterVolume = config.audio.masterVolume;
            _bgmVolume = config.audio.bgmVolume;
            _sfxVolume = config.audio.sfxVolume;
            _initialPoolSize = config.pool.audioInitialPoolSize;
            _maxPoolSize = config.pool.audioMaxPoolSize;
            UpdateAllVolumes();
        }

        #endregion
    }
}
