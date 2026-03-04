using UnityEngine;
using ObjectPool;

namespace Audio
{
    public class PooledAudioSource : MonoBehaviour, IGameObjectPoolObject
    {
        private AudioSource _audioSource;
        private AudioSource AudioSource => _audioSource != null ? _audioSource : (_audioSource = gameObject.AddComponent<AudioSource>());

        public bool IsPlaying => AudioSource != null && AudioSource.isPlaying;
        public AudioClip CurrentClip => AudioSource?.clip;

        public GameObject GameObject => gameObject;
        public Transform Transform => transform;

        public void OnGetFromPool()
        {
            gameObject.SetActive(true);
            AudioSource.playOnAwake = false;
            AudioSource.loop = false;
        }

        public void OnReleaseToPool()
        {
            AudioSource.Stop();
            AudioSource.clip = null;
            gameObject.SetActive(false);
        }

        public void OnDestroyFromPool()
        {
            Destroy(gameObject);
        }

        public void Play(AudioClip clip, float volume, float pitch, bool loop, float spatialBlend, int priority)
        {
            AudioSource.clip = clip;
            AudioSource.volume = volume;
            AudioSource.pitch = pitch;
            AudioSource.loop = loop;
            AudioSource.spatialBlend = spatialBlend;
            AudioSource.priority = priority;
            AudioSource.Play();
        }

        public void Stop()
        {
            AudioSource.Stop();
        }
    }
}
