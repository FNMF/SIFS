using System.Collections.Concurrent;

namespace SIFS.Shared.Extensions.EventBus
{
    public class EventBus : IEventBus
    {
        private readonly ConcurrentDictionary<string, List<Action<AppEvent>>> _listenersByEventType = new();
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<EventBus> _logger;

        public EventBus(IServiceScopeFactory scopeFactory, ILogger<EventBus> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        public void Register(string eventType, Action<AppEvent> listener)
        {
            if (string.IsNullOrWhiteSpace(eventType))
                throw new ArgumentException("Event type is required.", nameof(eventType));

            ArgumentNullException.ThrowIfNull(listener);

            var normalizedEventType = NormalizeEventType(eventType);
            var listeners = _listenersByEventType.GetOrAdd(normalizedEventType, _ => new List<Action<AppEvent>>());

            lock (listeners)
            {
                listeners.Add(listener);
            }
        }

        public void Publish(AppEvent appEvent)
        {
            ArgumentNullException.ThrowIfNull(appEvent);

            if (string.IsNullOrWhiteSpace(appEvent.EventType))
                throw new ArgumentException("Event type is required.", nameof(appEvent));

            if (appEvent.CreatedAt == default)
                appEvent.CreatedAt = DateTime.UtcNow;

            var normalizedEventType = NormalizeEventType(appEvent.EventType);
            appEvent.EventType = normalizedEventType;

            if (!_listenersByEventType.TryGetValue(normalizedEventType, out var listeners))
                return;

            Action<AppEvent>[] snapshot;
            lock (listeners)
            {
                snapshot = listeners.ToArray();
            }

            foreach (var listener in snapshot)
            {
                try
                {
                    listener(appEvent);
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "AppEvent listener failed. EventType={EventType}, ActorId={ActorId}, TargetType={TargetType}, TargetId={TargetId}",
                        appEvent.EventType,
                        appEvent.ActorId,
                        appEvent.TargetType,
                        appEvent.TargetId);
                }
            }
        }

        public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
            where TEvent : IDomainEvent
        {
            using var scope = _scopeFactory.CreateScope();
            var handlers = scope.ServiceProvider.GetServices<IEventHandler<TEvent>>();

            foreach (var handler in handlers)
            {
                try
                {
                    await handler.HandleAsync(@event, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Domain event handler failed. EventType={EventType}", typeof(TEvent).Name);
                }
            }
        }

        private static string NormalizeEventType(string eventType) => eventType.Trim().ToUpperInvariant();
    }
}
