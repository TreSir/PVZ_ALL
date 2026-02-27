using UnityEngine;
using Audio;

namespace Core
{
    /// <summary>
    /// 游戏初始化类
    /// </summary>
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
