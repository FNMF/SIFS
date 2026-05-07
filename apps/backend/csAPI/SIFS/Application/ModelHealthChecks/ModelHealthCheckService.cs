using System.Diagnostics;
using SIFS.Infrastructure.Persistence.Models;
using SIFS.Infrastructure.Repositories;
using SIFS.Shared.Extensions.EventBus;
using SIFS.Shared.Results;

namespace SIFS.Application.ModelHealthChecks
{
    public class ModelHealthCheckService : IModelHealthCheckService
    {
        private readonly IAlgoModelRepository _algoModelRepository;
        private readonly IModelHealthCheckRepository _healthCheckRepository;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IEventBus _eventBus;
        private readonly ILogger<ModelHealthCheckService> _logger;

        public ModelHealthCheckService(
            IAlgoModelRepository algoModelRepository,
            IModelHealthCheckRepository healthCheckRepository,
            IHttpClientFactory httpClientFactory,
            IEventBus eventBus,
            ILogger<ModelHealthCheckService> logger)
        {
            _algoModelRepository = algoModelRepository;
            _healthCheckRepository = healthCheckRepository;
            _httpClientFactory = httpClientFactory;
            _eventBus = eventBus;
            _logger = logger;
        }

        public async Task CheckAlgoHealthAsync(AlgoModel algoModel)
        {
            if (!algoModel.Enabled || algoModel.DeletedAt != null)
                return;

            var previous = await _healthCheckRepository.GetLatestByAlgoModelIdAsync(algoModel.Id);
            var checkedAt = DateTime.UtcNow;
            var stopwatch = Stopwatch.StartNew();
            var status = ModelHealthStatus.Offline;
            int? responseTimeMs = null;
            string? failureReason = null;

            if (!Uri.TryCreate(algoModel.ApiUrl, UriKind.Absolute, out var uri))
            {
                failureReason = "invalid algorithm api url";
            }
            else
            {
                try
                {
                    using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(2));
                    var client = _httpClientFactory.CreateClient();
                    using var response = await client.GetAsync(uri, timeout.Token);
                    stopwatch.Stop();
                    responseTimeMs = (int)Math.Min(stopwatch.ElapsedMilliseconds, int.MaxValue);

                    if (response.IsSuccessStatusCode)
                    {
                        status = ModelHealthStatus.Online;
                    }
                    else
                    {
                        status = ModelHealthStatus.Offline;
                        failureReason = $"non-2xx response: {(int)response.StatusCode}";
                    }
                }
                catch (OperationCanceledException)
                {
                    stopwatch.Stop();
                    status = ModelHealthStatus.Timeout;
                    responseTimeMs = (int)Math.Min(stopwatch.ElapsedMilliseconds, int.MaxValue);
                    failureReason = "algorithm health check timeout";
                }
                catch (Exception ex)
                {
                    stopwatch.Stop();
                    status = ModelHealthStatus.Offline;
                    responseTimeMs = stopwatch.ElapsedMilliseconds > 0 ? (int)Math.Min(stopwatch.ElapsedMilliseconds, int.MaxValue) : null;
                    failureReason = "algorithm health check failed";
                    _logger.LogDebug(ex, "Algorithm health check failed. AlgoId={AlgoId}, Url={Url}", algoModel.Id, algoModel.ApiUrl);
                }
            }

            try
            {
                await _healthCheckRepository.CreateAsync(new ModelHealthCheck
                {
                    AlgoModelId = algoModel.Id,
                    Status = status,
                    ResponseTimeMs = responseTimeMs,
                    CheckedAt = checkedAt,
                    FailureReason = failureReason
                });

                if (previous == null || previous.Status != status)
                {
                    PublishHealthChanged(algoModel, status, responseTimeMs, checkedAt);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Persisting algorithm health check failed. AlgoId={AlgoId}", algoModel.Id);
            }
        }

        public async Task CheckEnabledAlgosHealthAsync()
        {
            var page = 1;
            const int pageSize = 100;

            while (true)
            {
                var algos = await _algoModelRepository.ListAsync(new SIFS.Application.AlgoModels.AlgoModelQuery
                {
                    Enabled = true,
                    Page = page,
                    PageSize = pageSize
                });

                foreach (var algo in algos.Data)
                {
                    try
                    {
                        await CheckAlgoHealthAsync(new AlgoModel
                        {
                            Id = algo.Id,
                            Name = algo.Name,
                            Enabled = algo.Enabled,
                            ApiUrl = algo.ApiUrl,
                            Description = algo.Description,
                            ReservedJson = algo.ReservedJson,
                            CreatedAt = algo.CreatedAt,
                            UpdatedAt = algo.UpdatedAt
                        });
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Algorithm health check iteration failed. AlgoId={AlgoId}", algo.Id);
                    }
                }

                if (page * pageSize >= algos.Total || algos.Data.Count == 0)
                    break;

                page++;
            }
        }

        public async Task<ModelHealthStatusDto?> GetLatestAlgoHealthAsync(int algoModelId)
        {
            var algo = await _algoModelRepository.FindByIdAsync(algoModelId);
            if (algo == null)
                return null;

            var health = await _healthCheckRepository.GetLatestByAlgoModelIdAsync(algoModelId);
            return new ModelHealthStatusDto
            {
                AlgoModelId = algo.Id,
                Name = algo.Name,
                Enabled = algo.Enabled,
                ApiUrl = algo.ApiUrl,
                HealthStatus = algo.Enabled ? health?.Status ?? ModelHealthStatus.Offline : "disabled",
                ResponseTimeMs = health?.ResponseTimeMs,
                CheckedAt = health?.CheckedAt,
                FailureReason = health?.FailureReason,
                Description = algo.Description
            };
        }

        public Task<ModelHealthSummaryDto> GetDashboardAlgoHealthSummaryAsync()
        {
            return _healthCheckRepository.CountByLatestStatusAsync();
        }

        public Task<Paged<ModelHealthStatusDto>> ListAlgoHealthStatusesAsync(ModelHealthStatusQuery query)
        {
            return _healthCheckRepository.ListLatestStatusesAsync(query);
        }

        private void PublishHealthChanged(AlgoModel algoModel, string status, int? responseTimeMs, DateTime checkedAt)
        {
            try
            {
                _eventBus.Publish(new AppEvent
                {
                    EventType = AppEventTypes.AlgoHealthChanged,
                    TargetType = "algo",
                    TargetId = algoModel.Id.ToString(),
                    Payload = new Dictionary<string, object?>
                    {
                        ["algo_model_id"] = algoModel.Id,
                        ["name"] = algoModel.Name,
                        ["status"] = status,
                        ["response_time_ms"] = responseTimeMs,
                        ["checked_at"] = checkedAt
                    },
                    CreatedAt = checkedAt
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Publishing algorithm health changed event failed. AlgoId={AlgoId}", algoModel.Id);
            }
        }
    }
}
