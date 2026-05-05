using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SIFS.Application.Rbac;
using SIFS.Infrastructure.Identity;

namespace SIFS.Infrastructure.Authorization
{
    public class RequirePermissionFilter : IAsyncAuthorizationFilter
    {
        private readonly string _permissionCode;
        private readonly ICurrentService _currentService;
        private readonly IPermissionService _permissionService;

        public RequirePermissionFilter(
            string permissionCode,
            ICurrentService currentService,
            IPermissionService permissionService)
        {
            _permissionCode = permissionCode;
            _currentService = currentService;
            _permissionService = permissionService;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            if (!_currentService.IsAuthenticated || _currentService.CurrentUuid == null)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            var result = await _permissionService.HasPermissionAsync(
                _currentService.CurrentUuid.Value,
                _permissionCode);

            if (!result.IsSuccess || !result.Data)
            {
                context.Result = new ForbidResult();
            }
        }
    }
}
