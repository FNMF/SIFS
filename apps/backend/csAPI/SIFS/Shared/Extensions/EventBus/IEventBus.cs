namespace SIFS.Shared.Extensions.EventBus
{
    public interface IEventBus
    {
        Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default) where TEvent : IDomainEvent;
    }
}
