using Microsoft.AspNetCore.SignalR;
using SIFS.Shared.Extensions.EventBus;

namespace SIFS.Infrastructure.Realtime
{
    public class DashboardRealtimeListener
    {
        private readonly IHubContext<DashboardHub> _hubContext;
        private readonly ILogger<DashboardRealtimeListener> _logger;

        private static readonly IReadOnlyDictionary<string, string> EventMap = new Dictionary<string, string>
        {
            [AppEventTypes.UserLogin] = "operation.log.created",
            [AppEventTypes.TaskCreated] = "task.created",
            [AppEventTypes.TaskStatusChanged] = "task.status.changed",
            [AppEventTypes.TaskDeleted] = "dashboard.summary.changed",
            [AppEventTypes.TaskRetried] = "dashboard.summary.changed",
            [AppEventTypes.TaskViewed] = "dashboard.summary.changed",
            [AppEventTypes.AlgoCreated] = "algo.status.changed",
            [AppEventTypes.AlgoUpdated] = "algo.status.changed",
            [AppEventTypes.AlgoEnabled] = "algo.status.changed",
            [AppEventTypes.AlgoDisabled] = "algo.status.changed",
            [AppEventTypes.AlgoHealthChanged] = "algo.health.changed",
            [AppEventTypes.ResultDownloaded] = "operation.log.created"
        };

        public DashboardRealtimeListener(
            IHubContext<DashboardHub> hubContext,
            ILogger<DashboardRealtimeListener> logger)
        {
            _hubContext = hubContext;
            _logger = logger;
        }

        public void Register(IEventBus eventBus)
        {
            foreach (var eventType in EventMap.Keys)
            {
                eventBus.Register(eventType, Handle);
            }
        }

        public void Handle(AppEvent appEvent)
        {
            try
            {
                var signalEvent = EventMap.TryGetValue(appEvent.EventType, out var mapped)
                    ? mapped
                    : "dashboard.summary.changed";

                if (appEvent.EventType != AppEventTypes.ResultDownloaded && appEvent.EventType != AppEventTypes.TaskViewed)
                {
                    Push("dashboard.summary.changed", appEvent, "dashboard.summary.changed");
                }

                Push(signalEvent, appEvent, signalEvent);

                if (appEvent.EventType != AppEventTypes.TaskStatusChanged && signalEvent != "operation.log.created")
                {
                    Push("operation.log.created", appEvent, "operation.log.created");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Dashboard realtime push failed. EventType={EventType}", appEvent.EventType);
            }
        }

        private void Push(string method, AppEvent appEvent, string signalEvent)
        {
            var payload = new
            {
                @event = signalEvent,
                task_id = appEvent.TargetType == "detection_task" ? appEvent.TargetId : null,
                target_type = appEvent.TargetType,
                target_id = appEvent.TargetId,
                status = GetPayloadValue(appEvent, "to_status"),
                health_status = GetPayloadValue(appEvent, "status"),
                response_time_ms = GetPayloadValue(appEvent, "response_time_ms"),
                operation_type = appEvent.EventType,
                created_at = appEvent.CreatedAt
            };

            _hubContext.Clients
                .Group(DashboardHub.AdminDashboardGroup)
                .SendAsync(method, payload)
                .GetAwaiter()
                .GetResult();

            if (method != "dashboard.message")
            {
                _hubContext.Clients
                    .Group(DashboardHub.AdminDashboardGroup)
                    .SendAsync("dashboard.message", payload)
                    .GetAwaiter()
                    .GetResult();
            }
        }

        private static object? GetPayloadValue(AppEvent appEvent, string key)
        {
            return appEvent.Payload != null && appEvent.Payload.TryGetValue(key, out var value)
                ? value
                : null;
        }
    }
}
