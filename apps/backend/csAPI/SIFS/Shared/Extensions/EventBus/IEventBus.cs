namespace SIFS.Shared.Extensions.EventBus
{
    public interface IEventBus
    {
        void Register(string eventType, Action<AppEvent> listener);

        void Publish(AppEvent appEvent);

        Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default) where TEvent : IDomainEvent;
    }
}
