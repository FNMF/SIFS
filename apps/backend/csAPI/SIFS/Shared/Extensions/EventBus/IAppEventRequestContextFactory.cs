namespace SIFS.Shared.Extensions.EventBus
{
    public interface IAppEventRequestContextFactory
    {
        Dictionary<string, object?> Create(string? requestSummary = null, string? actorUsername = null);
    }
}
