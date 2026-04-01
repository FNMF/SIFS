namespace SIFS.Shared.Extensions.EventBus
{
    public interface IDomainEvent
    {
        DateTime OccurredOn { get; }
    }
}
