using System.Text.Json;
using SIFS.Infrastructure.Persistence.Models;
using SIFS.Infrastructure.Repositories;
using SIFS.Shared.Extensions.EventBus;
using SIFS.Shared.Helpers;
using SIFS.Shared.Results;

namespace SIFS.Application.OperationLogs
{
    public class OperationLogService : IOperationLogService
    {
        private readonly IOperationLogRepository _operationLogRepository;

        public OperationLogService(IOperationLogRepository operationLogRepository)
        {
            _operationLogRepository = operationLogRepository;
        }

        public async Task RecordFromEventAsync(AppEvent appEvent)
        {
            var requestContext = appEvent.RequestContext;

            var operationLog = new OperationLog
            {
                Id = UuidV7.NewUuidV7(),
                ActorId = appEvent.ActorId,
                ActorUsername = GetContextString(requestContext, "actor_username"),
                OperationType = appEvent.EventType,
                TargetType = appEvent.TargetType,
                TargetId = appEvent.TargetId,
                RequestIp = GetContextString(requestContext, "request_ip"),
                RequestMethod = GetContextString(requestContext, "request_method"),
                RequestPath = GetContextString(requestContext, "request_path"),
                RequestSummary = GetRequestSummary(appEvent),
                Success = appEvent.Success,
                FailureReason = appEvent.ErrorMessage,
                CreatedAt = appEvent.CreatedAt == default ? DateTime.UtcNow : appEvent.CreatedAt
            };

            await _operationLogRepository.CreateAsync(operationLog);
        }

        public async Task<Result<Paged<OperationLogDto>>> QueryLogsAsync(OperationLogQuery query)
        {
            var page = await _operationLogRepository.QueryAsync(query);
            return Result<Paged<OperationLogDto>>.Success(page);
        }

        private static string? GetContextString(Dictionary<string, object?>? context, string key)
        {
            if (context == null || !context.TryGetValue(key, out var value) || value == null)
                return null;

            return value.ToString();
        }

        private static string? GetRequestSummary(AppEvent appEvent)
        {
            var contextSummary = GetContextString(appEvent.RequestContext, "request_summary");
            if (!string.IsNullOrWhiteSpace(contextSummary))
                return contextSummary;

            if (appEvent.Payload == null || appEvent.Payload.Count == 0)
                return null;

            return JsonSerializer.Serialize(appEvent.Payload);
        }
    }
}
