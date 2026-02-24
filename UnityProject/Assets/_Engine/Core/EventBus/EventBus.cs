using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace GameEngine.Core.EventBus
{
    /// <summary>
    /// Asynchronous event bus with typed payloads.
    /// Subscribers receive events via async handlers.
    /// </summary>
    public sealed class EventBus
    {
        private readonly Dictionary<Type, List<Delegate>> _handlers = new();
        private readonly object _lock = new();

        public void Subscribe<T>(Func<T, CancellationToken, Task> handler) where T : IEvent
        {
            var type = typeof(T);
            lock (_lock)
            {
                if (!_handlers.TryGetValue(type, out var list))
                {
                    list = new List<Delegate>();
                    _handlers[type] = list;
                }
                list.Add(handler);
            }
        }

        public void Unsubscribe<T>(Func<T, CancellationToken, Task> handler) where T : IEvent
        {
            var type = typeof(T);
            lock (_lock)
            {
                if (_handlers.TryGetValue(type, out var list))
                {
                    list.Remove(handler);
                    if (list.Count == 0)
                        _handlers.Remove(type);
                }
            }
        }

        public async Task PublishAsync<T>(T payload, CancellationToken cancellationToken = default) where T : IEvent
        {
            List<Delegate> handlers;
            lock (_lock)
            {
                if (!_handlers.TryGetValue(typeof(T), out var list) || list.Count == 0)
                    return;
                handlers = new List<Delegate>(list);
            }

            foreach (var handler in handlers)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                var func = (Func<T, CancellationToken, Task>)handler;
                await func(payload, cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
