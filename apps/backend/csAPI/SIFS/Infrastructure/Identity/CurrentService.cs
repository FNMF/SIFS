using System.Security.Claims;

namespace SIFS.Infrastructure.Identity
{
    public class CurrentService : ICurrentService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentService(IHttpContextAccessor accessor)
        {
            _httpContextAccessor = accessor;
        }

        private ClaimsPrincipal? ClaimsPrincipal => _httpContextAccessor.HttpContext?.User;
        public bool IsAuthenticated => ClaimsPrincipal?.Identity?.IsAuthenticated ?? false;
        public Guid? CurrentUuid
        {
            get
            {
                var uuidString = GetClaim(ClaimTypes.NameIdentifier);
                Guid.TryParse(uuidString, out var guid);
                return guid;
            }
        }
        public Guid RequiredUuid
        {
            get
            {
                var uuid = CurrentUuid;
                if (uuid == null)
                {
                    throw new UnauthorizedAccessException("Current UUID is required but not found.");
                }
                return uuid.Value;
            }
        }
        public string? CurrentAccount => GetClaim("Account");

        private string? GetClaim(string claimType) =>
            ClaimsPrincipal?.Claims.FirstOrDefault(c => c.Type == claimType)?.Value;
    }
}
