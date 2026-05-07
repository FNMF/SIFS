using SIFS.Application.ModelHealthChecks;

namespace SIFS.Infrastructure
{
    public class ModelHealthCheckWorker : BackgroundService
    {
        private static readonly TimeSpan Interval = TimeSpan.FromMinutes(1);
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<ModelHealthCheckWorker> _logger;

        public ModelHealthCheckWorker(IServiceScopeFactory scopeFactory, ILogger<ModelHealthCheckWorker> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var service = scope.ServiceProvider.GetRequiredService<IModelHealthCheckService>();
                    await service.CheckEnabledAlgosHealthAsync();
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Scheduled algorithm health check job failed.");
                }

                try
                {
                    await Task.Delay(Interval, stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
            }
        }
    }
}
