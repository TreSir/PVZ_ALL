using System;
using System.Collections.Generic;
using System.Threading;

namespace EventBus
{
    public interface IEvent { }

    public interface IEventHandler
    {
        void Handle(IEvent e);
    }

    public class EventHandler<T> : IEventHandler where T : IEvent
    {
        private readonly Action<T> _handler;
        private readonly int _priority;

        public EventHandler(Action<T> handler, int priority = 0)
        {
            _handler = handler ?? throw new ArgumentNullException(nameof(handler));
            _priority = priority;
        }

        public int Priority => _priority;

        public void Handle(IEvent e)
        {
            _handler((T)e);
        }
    }

    public static class EventBus
    {
        private static readonly Dictionary<Type, List<IEventHandler>> _handlers = new Dictionary<Type, List<IEventHandler>>();
        private static readonly Dictionary<Type, List<IEventHandler>> _pendingHandlers = new Dictionary<Type, List<IEventHandler>>();
        private static readonly object _lock = new object();
        private static int _pendingCount;

        public static void Subscribe<T>(Action<T> handler, int priority = 0) where T : IEvent
        {
            if (handler == null) throw new ArgumentNullException(nameof(handler));

            var type = typeof(T);
            lock (_lock)
            {
                if (!_handlers.TryGetValue(type, out var handlers))
                {
                    handlers = new List<IEventHandler>();
                    _handlers[type] = handlers;
                }

                var eventHandler = new EventHandler<T>(handler, priority);
                handlers.Add(eventHandler);

                handlers.Sort((a, b) => b.Priority.CompareTo(a.Priority));
            }
        }

        public static void Unsubscribe<T>(Action<T> handler) where T : IEvent
        {
            if (handler == null) return;

            var type = typeof(T);
            lock (_lock)
            {
                if (_handlers.TryGetValue(type, out var handlers))
                {
                    for (int i = handlers.Count - 1; i >= 0; i--)
                    {
                        if (handlers[i] is EventHandler<T> typedHandler && typedHandler.Handle != null)
                        {
                            handlers.RemoveAt(i);
                            break;
                        }
                    }
                }
            }
        }

        public static void Publish<T>(T eventData) where T : IEvent
        {
            var type = typeof(T);
            List<IEventHandler> handlersCopy = null;

            lock (_lock)
            {
                if (!_handlers.TryGetValue(type, out var handlers) || handlers.Count == 0)
                {
                    return;
                }

                handlersCopy = new List<IEventHandler>(handlers);
            }

            for (int i = 0; i < handlersCopy.Count; i++)
            {
                try
                {
                    handlersCopy[i].Handle(eventData);
                }
                catch (Exception ex)
                {
                    UnityEngine.Debug.LogError($"[EventBus] Error handling event {type.Name}: {ex.Message}");
                }
            }
        }

        public static void PublishAsync<T>(T eventData) where T : IEvent
        {
            var type = typeof(T);
            List<IEventHandler> handlersCopy = null;

            lock (_lock)
            {
                if (!_handlers.TryGetValue(type, out var handlers) || handlers.Count == 0)
                {
                    return;
                }

                handlersCopy = new List<IEventHandler>(handlers);
            }

            ThreadPool.QueueUserWorkItem(_ =>
            {
                for (int i = 0; i < handlersCopy.Count; i++)
                {
                    try
                    {
                        handlersCopy[i].Handle(eventData);
                    }
                    catch (Exception ex)
                    {
                        UnityEngine.Debug.LogError($"[EventBus] Async error handling event {type.Name}: {ex.Message}");
                    }
                }
            });
        }

        public static void Clear()
        {
            lock (_lock)
            {
                _handlers.Clear();
                _pendingHandlers.Clear();
                _pendingCount = 0;
            }
        }

        public static void Clear<T>() where T : IEvent
        {
            var type = typeof(T);
            lock (_lock)
            {
                if (_handlers.TryGetValue(type, out var handlers))
                {
                    handlers.Clear();
                }
            }
        }

        public static int GetHandlerCount<T>() where T : IEvent
        {
            var type = typeof(T);
            lock (_lock)
            {
                if (_handlers.TryGetValue(type, out var handlers))
                {
                    return handlers.Count;
                }
            }
            return 0;
        }

        public static bool HasHandler<T>() where T : IEvent
        {
            return GetHandlerCount<T>() > 0;
        }
    }
}
