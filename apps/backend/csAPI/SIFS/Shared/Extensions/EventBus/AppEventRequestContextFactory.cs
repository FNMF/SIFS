using SIFS.Infrastructure.Identity;

namespace SIFS.Shared.Extensions.EventBus
{
    public class AppEventRequestContextFactory : IAppEventRequestContextFactory
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ICurrentService _currentService;

        public AppEventRequestContextFactory(
            IHttpContextAccessor httpContextAccessor,
            ICurrentService currentService)
        {
            _httpContextAccessor = httpContextAccessor;
            _currentService = currentService;
        }

        public Dictionary<string, object?> Create(string? requestSummary = null, string? actorUsername = null)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            var request = httpContext?.Request;

            return new Dictionary<string, object?>
            {
                ["actor_username"] = actorUsername ?? _currentService.CurrentAccount,
                ["request_ip"] = httpContext?.Connection.RemoteIpAddress?.ToString(),
                ["request_method"] = request?.Method,
                ["request_path"] = request?.Path.Value,
                ["request_summary"] = requestSummary
            };
        }
    }
}
