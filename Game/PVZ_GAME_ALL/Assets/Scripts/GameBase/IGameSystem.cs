namespace GameBase
{
    public interface IGameSystem
    {
        int Priority { get; }
        void Initialize();
        void Shutdown();
    }

    public interface IGameSystemPreload : IGameSystem
    {
        void Preload();
    }

    public interface IGameSystemUpdate : IGameSystem
    {
        void Update(float deltaTime);
    }

    public interface IGameSystemLateUpdate : IGameSystem
    {
        void LateUpdate(float deltaTime);
    }
}
