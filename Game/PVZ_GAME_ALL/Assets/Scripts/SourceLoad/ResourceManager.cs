using System;
using UnityEngine;

namespace SourceLoad
{
    public static class ResourceManager
    {
        private static IResourceLoader _loader;
        private static bool _isInitialized;

        public static bool IsInitialized => _isInitialized;

        static ResourceManager()
        {
            Initialize();
        }

        public static void Initialize()
        {
            if (_isInitialized) return;

            _loader = new ResourcesLoader();
            _isInitialized = true;
        }

        public static void SetLoader(IResourceLoader loader)
        {
            _loader = loader;
        }

        public static T Load<T>(string path) where T : UnityEngine.Object
        {
            if (_loader == null)
            {
                Debug.LogError("[ResourceManager] Loader is not initialized.");
                return null;
            }
            return _loader.Load<T>(path);
        }

        public static void LoadAsync<T>(string path, Action<T> onComplete) where T : UnityEngine.Object
        {
            if (_loader == null)
            {
                Debug.LogError("[ResourceManager] Loader is not initialized.");
                onComplete?.Invoke(null);
                return;
            }
            _loader.LoadAsync(path, onComplete);
        }

        public static void Unload(UnityEngine.Object asset)
        {
            _loader?.Unload(asset);
        }
    }
}
