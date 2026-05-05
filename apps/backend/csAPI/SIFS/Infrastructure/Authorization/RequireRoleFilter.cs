using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SIFS.Application.Rbac;
using SIFS.Infrastructure.Identity;

namespace SIFS.Infrastructure.Authorization
{
    public class RequireRoleFilter : IAsyncAuthorizationFilter
    {
        private readonly string _roleName;
        private readonly ICurrentService _currentService;
        private readonly IPermissionService _permissionService;

        public RequireRoleFilter(
            string roleName,
            ICurrentService currentService,
            IPermissionService permissionService)
        {
            _roleName = roleName;
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

            var result = await _permissionService.HasRoleAsync(
                _currentService.CurrentUuid.Value,
                _roleName);

            if (!result.IsSuccess || !result.Data)
            {
                context.Result = new ForbidResult();
            }
        }
    }
}
