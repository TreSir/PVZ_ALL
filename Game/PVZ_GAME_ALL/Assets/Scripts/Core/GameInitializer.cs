using UnityEngine;
using Audio;

namespace Core
{
    public class GameInitializer : MonoBehaviour
    {
        private void Awake()
        {
            InitializeGameSystems();
        }

        private void InitializeGameSystems()
        {
            AudioManager.Instance.Initialize();
        }
    }
}
