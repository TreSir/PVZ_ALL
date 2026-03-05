using System.Collections.Generic;
using UnityEngine;
using Config;
using SourceLoad;
using ObjectPool;

namespace Audio
{
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

        private const string PoolKey = "AudioSourcePool";
        private const string PoolConfigPath = "Configs/GameSettingConfig";

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

        private AudioSource _bgmSource;
        private GameObjectPool _sfxPool;
        private Transform _poolRoot;
        private List<PooledAudioSource> _activeSources = new List<PooledAudioSource>();
        private PoolConfig _poolConfig;
        private GameObject _pooledAudioSourcePrefab;
        private bool _isInitialized;

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
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                CleanupPools();
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
            if (_pooledAudioSourcePrefab == null)
            {
                _pooledAudioSourcePrefab = CreatePooledAudioSourcePrefab();
            }

            _poolRoot = new GameObject("[SFX_Pool]").transform;
            _poolRoot.SetParent(transform);

            if (_poolConfig == null)
            {
                _poolConfig = PoolConfig.Default;
            }

            if (ObjectPoolManager.Instance != null)
            {
                _sfxPool = ObjectPoolManager.Instance.GetOrCreatePool(
                    PoolKey,
                    _pooledAudioSourcePrefab,
                    _poolConfig);
            }
        }

        private GameObject CreatePooledAudioSourcePrefab()
        {
            var go = new GameObject("PooledAudioSource");
            var component = go.AddComponent<PooledAudioSource>();
            go.SetActive(false);
            return go;
        }

        private void UpdateAllVolumes()
        {
            if (_bgmSource != null)
            {
                _bgmSource.volume = _masterVolume * _bgmVolume;
            }

            foreach (var source in _activeSources)
            {
                if (source != null && source.IsPlaying)
                {
                    source.AudioSourceComponent.volume = _masterVolume * _sfxVolume;
                }
            }
        }

        private void RecycleFinishedSFX()
        {
            for (int i = _activeSources.Count - 1; i >= 0; i--)
            {
                var source = _activeSources[i];
                if (source == null || !source.IsPlaying)
                {
                    if (source != null && _sfxPool != null)
                    {
                        _sfxPool.Release(source.GameObject);
                    }
                    _activeSources.RemoveAt(i);
                }
            }
        }

        private PooledAudioSource GetAvailableSFXSource()
        {
            if (_sfxPool == null || _pooledAudioSourcePrefab == null)
            {
                InitializeSFXPool();
            }

            var go = _sfxPool.Get();
            var pooledSource = go.GetComponent<PooledAudioSource>();
            pooledSource.OnGetFromPool();
            _activeSources.Add(pooledSource);
            return pooledSource;
        }

        #region Public Methods

        /// <summary>
        /// 初始化音频管理器
        /// </summary>
        public void Initialize()
        {
            if (_isInitialized)
            {
                Debug.LogWarning("[AudioManager] Already initialized");
                return;
            }

            var config = ResourceManager.Load<GameSettingConfig>(PoolConfigPath);
            if (config == null)
            {
                Debug.LogWarning($"[AudioManager] Config not found at Resources/{PoolConfigPath}");
                _poolConfig = PoolConfig.Default;
            }
            else
            {
                _masterVolume = config.audio.masterVolume;
                _bgmVolume = config.audio.bgmVolume;
                _sfxVolume = config.audio.sfxVolume;

                _poolConfig = new PoolConfig
                {
                    initialSize = config.pool.audioInitialPoolSize,
                    maxSize = config.pool.audioMaxPoolSize,
                    autoExpand = true,
                    autoReleaseOnSceneChange = true
                };
            }

            InitializeSFXPool();
            UpdateAllVolumes();
            _isInitialized = true;

            Debug.Log("[AudioManager] Initialized successfully");
        }

        /// <summary>
        /// 关闭音频管理器
        /// </summary>
        public void Shutdown()
        {
            StopAll();
            CleanupPools();
            Debug.Log("[AudioManager] Shutdown");
        }

        #endregion

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

            source.Play(
                clip,
                _masterVolume * _sfxVolume * config.volume,
                config.pitch,
                config.loop,
                config.spatialBlend,
                config.priority);

            return source.AudioSourceComponent;
        }

        public AudioSource PlaySFXAtPosition(AudioClip clip, Vector3 position, SoundConfig config = null)
        {
            if (clip == null) return null;

            config = config ?? SoundConfig.SFX;

            var source = GetAvailableSFXSource();
            if (source == null) return null;

            source.Transform.position = position;
            source.Play(
                clip,
                _masterVolume * _sfxVolume * config.volume,
                config.pitch,
                config.loop,
                1f,
                config.priority);

            return source.AudioSourceComponent;
        }

        public void StopAllSFX()
        {
            foreach (var source in _activeSources)
            {
                if (source != null && _sfxPool != null)
                {
                    _sfxPool.Release(source.GameObject);
                }
            }
            _activeSources.Clear();
        }

        public void StopSFX(PooledAudioSource source)
        {
            if (source == null || _sfxPool == null) return;

            if (_activeSources.Contains(source))
            {
                _sfxPool.Release(source.GameObject);
                _activeSources.Remove(source);
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
            foreach (var source in _activeSources)
            {
                if (source != null && source.IsPlaying)
                {
                    source.AudioSourceComponent.Pause();
                }
            }
        }

        public void ResumeAll()
        {
            ResumeBGM();
            foreach (var source in _activeSources)
            {
                if (source != null && !source.IsPlaying && source.CurrentClip != null)
                {
                    source.AudioSourceComponent.UnPause();
                }
            }
        }

        #endregion

        #region Settings

        public void SetVolumeSettings(float masterVolume, float bgmVolume, float sfxVolume)
        {
            _masterVolume = Mathf.Clamp01(masterVolume);
            _bgmVolume = Mathf.Clamp01(bgmVolume);
            _sfxVolume = Mathf.Clamp01(sfxVolume);
            UpdateAllVolumes();
        }

        private void CleanupPools()
        {
            if (_sfxPool != null)
            {
                _sfxPool.ReleaseAll();
            }
            _activeSources.Clear();

            if (_poolRoot != null)
            {
                Destroy(_poolRoot.gameObject);
            }
        }

        #endregion
    }
}
