using SIFS.Application.OperationLogs;

namespace SIFS.Shared.Extensions.EventBus
{
    public class OperationLogListener
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<OperationLogListener> _logger;

        public OperationLogListener(
            IServiceScopeFactory scopeFactory,
            ILogger<OperationLogListener> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        public void Handle(AppEvent appEvent)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var service = scope.ServiceProvider.GetRequiredService<IOperationLogService>();
                service.RecordFromEventAsync(appEvent).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to persist operation log. EventType={EventType}", appEvent.EventType);
            }
        }

        public void RegisterAll(IEventBus eventBus)
        {
            foreach (var eventType in AppEventTypes.All)
            {
                eventBus.Register(eventType, Handle);
            }
        }
    }
}
