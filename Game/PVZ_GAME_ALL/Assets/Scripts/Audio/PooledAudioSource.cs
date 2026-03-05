using UnityEngine;
using ObjectPool;

namespace Audio
{
    public class PooledAudioSource : MonoBehaviour, IGameObjectPoolObject
    {
        private AudioSource _audioSource;

        public AudioSource AudioSourceComponent
        {
            get
            {
                if (_audioSource == null)
                {
                    _audioSource = gameObject.AddComponent<AudioSource>();
                }
                return _audioSource;
            }
        }

        public bool IsPlaying => AudioSourceComponent != null && AudioSourceComponent.isPlaying;
        public AudioClip CurrentClip => AudioSourceComponent?.clip;

        public GameObject GameObject => gameObject;
        public Transform Transform => transform;

        public void OnGetFromPool()
        {
            gameObject.SetActive(true);
            AudioSourceComponent.playOnAwake = false;
            AudioSourceComponent.loop = false;
        }

        public void OnReleaseToPool()
        {
            AudioSourceComponent.Stop();
            AudioSourceComponent.clip = null;
            gameObject.SetActive(false);
        }

        public void OnDestroyFromPool()
        {
            Destroy(gameObject);
        }

        public void Play(AudioClip clip, float volume, float pitch, bool loop, float spatialBlend, int priority)
        {
            AudioSourceComponent.clip = clip;
            AudioSourceComponent.volume = volume;
            AudioSourceComponent.pitch = pitch;
            AudioSourceComponent.loop = loop;
            AudioSourceComponent.spatialBlend = spatialBlend;
            AudioSourceComponent.priority = priority;
            AudioSourceComponent.Play();
        }

        public void Stop()
        {
            AudioSourceComponent.Stop();
        }
    }
}
