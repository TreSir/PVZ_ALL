using System;

namespace SourceLoad
{
    public interface IResourceLoader
    {
        T Load<T>(string path) where T : UnityEngine.Object;
        void LoadAsync<T>(string path, Action<T> onComplete) where T : UnityEngine.Object;
        void Unload(UnityEngine.Object asset);
    }
}
