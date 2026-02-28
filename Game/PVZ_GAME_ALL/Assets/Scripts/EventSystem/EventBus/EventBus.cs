using System;
using System.Collections.Generic;

namespace EventBus
{
    public static class EventBus
    {
        private static readonly Dictionary<Type, List<Delegate>> _handlers = new Dictionary<Type, List<Delegate>>();

        public static void Subscribe<T>(Action<T> handler) where T : IEvent
        {
            var type = typeof(T);
            if (!_handlers.TryGetValue(type, out var handlers))
            {
                handlers = new List<Delegate>();
                _handlers[type] = handlers;
            }
            handlers.Add(handler);
        }

        public static void Unsubscribe<T>(Action<T> handler) where T : IEvent
        {
            var type = typeof(T);
            if (_handlers.TryGetValue(type, out var handlers))
            {
                handlers.Remove(handler);
            }
        }

        public static void Publish<T>(T eventData) where T : IEvent
        {
            var type = typeof(T);
            if (_handlers.TryGetValue(type, out var handlers))
            {
                for (int i = handlers.Count - 1; i >= 0; i--)
                {
                    (handlers[i] as Action<T>)?.Invoke(eventData);
                }
            }
        }

        public static void Clear()
        {
            _handlers.Clear();
        }

        public static void Clear<T>() where T : IEvent
        {
            var type = typeof(T);
            if (_handlers.TryGetValue(type, out var handlers))
            {
                handlers.Clear();
            }
        }
    }
}
