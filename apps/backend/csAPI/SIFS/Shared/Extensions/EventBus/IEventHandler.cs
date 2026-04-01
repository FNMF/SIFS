namespace SIFS.Shared.Extensions.EventBus
{
    public interface IEventHandler<TEvent> where TEvent : IDomainEvent
    {
        Task HandleAsync(TEvent @event, CancellationToken cancellationToken = default);
    }
}
