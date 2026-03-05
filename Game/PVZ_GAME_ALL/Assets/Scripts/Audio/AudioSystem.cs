using UnityEngine;
using GameBase;

namespace Audio
{
    public class AudioSystem : IGameSystem
    {
        public int Priority => 1000;

        public void Initialize()
        {
            Debug.Log("[AudioSystem] Initializing...");
            var manager = AudioManager.Instance;
            if (manager != null)
            {
                manager.Initialize();
            }
            else
            {
                Debug.LogError("[AudioSystem] Failed to create AudioManager instance");
            }
        }

        public void Shutdown()
        {
            Debug.Log("[AudioSystem] Shutting down...");
            if (AudioManager.HasInstance)
            {
                AudioManager.Instance.Shutdown();
            }
        }
    }
}
