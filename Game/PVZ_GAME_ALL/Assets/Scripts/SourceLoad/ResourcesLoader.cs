using System;
using UnityEngine;

namespace SourceLoad
{
    public class ResourcesLoader : IResourceLoader
    {
        public T Load<T>(string path) where T : UnityEngine.Object
        {
            return Resources.Load<T>(path);
        }

        public void LoadAsync<T>(string path, Action<T> onComplete) where T : UnityEngine.Object
        {
            var request = Resources.LoadAsync<T>(path);
            request.completed += _ =>
            {
                onComplete?.Invoke(request.asset as T);
            };
        }

        public void Unload(UnityEngine.Object asset)
        {
            if (asset != null)
            {
                Resources.UnloadAsset(asset);
            }
        }
    }
}
