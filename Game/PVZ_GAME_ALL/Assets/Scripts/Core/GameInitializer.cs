using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Core
{
    public class GameInitializer : MonoBehaviour
    {
        private static GameInitializer _instance;
        private static readonly object _lock = new object();
        private static bool _applicationIsQuitting;

        private readonly List<IGameSystem> _systems = new List<IGameSystem>();
        private readonly List<IGameSystemUpdate> _updateSystems = new List<IGameSystemUpdate>();
        private readonly List<IGameSystemLateUpdate> _lateUpdateSystems = new List<IGameSystemLateUpdate>();

        public static GameInitializer Instance
        {
            get
            {
                if (_applicationIsQuitting)
                {
                    Debug.LogWarning("[GameInitializer] Application is quitting, returning null.");
                    return null;
                }

                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = FindObjectOfType<GameInitializer>();

                        if (_instance == null)
                        {
                            var go = new GameObject("[GameInitializer]");
                            _instance = go.AddComponent<GameInitializer>();
                            DontDestroyOnLoad(go);
                        }
                    }

                    return _instance;
                }
            }
        }

        public static bool HasInstance => _instance != null && !_applicationIsQuitting;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);

            DiscoverAndInitializeSystems();
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                ShutdownAllSystems();
                _applicationIsQuitting = true;
            }
        }

        private void OnApplicationQuit()
        {
            _applicationIsQuitting = true;
        }

        private void Update()
        {
            float deltaTime = Time.deltaTime;
            for (int i = 0; i < _updateSystems.Count; i++)
            {
                _updateSystems[i].Update(deltaTime);
            }
        }

        private void LateUpdate()
        {
            float deltaTime = Time.deltaTime;
            for (int i = 0; i < _lateUpdateSystems.Count; i++)
            {
                _lateUpdateSystems[i].LateUpdate(deltaTime);
            }
        }

        public void DiscoverAndInitializeSystems()
        {
            var systemTypes = DiscoverGameSystems();

            foreach (var type in systemTypes)
            {
                RegisterSystem(type);
            }

            InitializeAllSystems();
        }

        private List<System.Type> DiscoverGameSystems()
        {
            var types = new List<System.Type>();
            var assemblies = System.AppDomain.CurrentDomain.GetAssemblies();

            foreach (var assembly in assemblies)
            {
                try
                {
                    foreach (var type in assembly.GetTypes())
                    {
                        if (type.IsClass &&
                            !type.IsAbstract &&
                            typeof(IGameSystem).IsAssignableFrom(type) &&
                            type != typeof(GameInitializer))
                        {
                            types.Add(type);
                        }
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"[GameInitializer] Failed to scan assembly {assembly.FullName}: {e.Message}");
                }
            }

            return types;
        }

        private void RegisterSystem(System.Type type)
        {
            try
            {
                var system = System.Activator.CreateInstance(type) as IGameSystem;
                if (system != null)
                {
                    _systems.Add(system);

                    if (system is IGameSystemUpdate updateSystem)
                    {
                        _updateSystems.Add(updateSystem);
                    }

                    if (system is IGameSystemLateUpdate lateUpdateSystem)
                    {
                        _lateUpdateSystems.Add(lateUpdateSystem);
                    }

                    Debug.Log($"[GameInitializer] Registered system: {type.Name}");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[GameInitializer] Failed to register system {type.Name}: {e.Message}");
            }
        }

        private void InitializeAllSystems()
        {
            var sortedSystems = _systems.OrderBy(s => s.Priority).ToList();

            foreach (var system in sortedSystems)
            {
                try
                {
                    Debug.Log($"[GameInitializer] Initializing: {system.GetType().Name}");
                    system.Initialize();
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"[GameInitializer] Failed to initialize {system.GetType().Name}: {e.Message}");
                }
            }

            foreach (var system in sortedSystems)
            {
                if (system is IGameSystemPreload preloadSystem)
                {
                    try
                    {
                        preloadSystem.Preload();
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError($"[GameInitializer] Failed to preload {system.GetType().Name}: {e.Message}");
                    }
                }
            }
        }

        private void ShutdownAllSystems()
        {
            var sortedSystems = _systems.OrderByDescending(s => s.Priority).ToList();

            foreach (var system in sortedSystems)
            {
                try
                {
                    system.Shutdown();
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"[GameInitializer] Failed to shutdown {system.GetType().Name}: {e.Message}");
                }
            }

            _systems.Clear();
            _updateSystems.Clear();
            _lateUpdateSystems.Clear();
        }

        public void AddSystem<T>() where T : IGameSystem, new()
        {
            var system = new T();
            _systems.Add(system);

            if (system is IGameSystemUpdate updateSystem)
            {
                _updateSystems.Add(updateSystem);
            }

            if (system is IGameSystemLateUpdate lateUpdateSystem)
            {
                _lateUpdateSystems.Add(lateUpdateSystem);
            }

            system.Initialize();
        }

        public void RemoveSystem<T>() where T : IGameSystem
        {
            IGameSystem toRemove = null;
            foreach (var system in _systems)
            {
                if (system is T)
                {
                    toRemove = system;
                    break;
                }
            }

            if (toRemove != null)
            {
                toRemove.Shutdown();
                _systems.Remove(toRemove);

                if (toRemove is IGameSystemUpdate updateSystem)
                {
                    _updateSystems.Remove(updateSystem);
                }

                if (toRemove is IGameSystemLateUpdate lateUpdateSystem)
                {
                    _lateUpdateSystems.Remove(lateUpdateSystem);
                }
            }
        }
    }
}
