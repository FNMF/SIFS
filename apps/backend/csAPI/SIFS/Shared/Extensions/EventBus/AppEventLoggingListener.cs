namespace SIFS.Shared.Extensions.EventBus
{
    public class AppEventLoggingListener
    {
        private readonly ILogger<AppEventLoggingListener> _logger;

        public AppEventLoggingListener(ILogger<AppEventLoggingListener> logger)
        {
            _logger = logger;
        }

        public void Handle(AppEvent appEvent)
        {
            _logger.LogInformation(
                "AppEvent {EventType} ActorId={ActorId} TargetType={TargetType} TargetId={TargetId} Success={Success} ErrorMessage={ErrorMessage}",
                appEvent.EventType,
                appEvent.ActorId,
                appEvent.TargetType,
                appEvent.TargetId,
                appEvent.Success,
                appEvent.ErrorMessage);
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
