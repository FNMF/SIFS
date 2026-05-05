using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIFS.Application.Rbac;
using SIFS.Infrastructure.Authorization;
using SIFS.Infrastructure.Identity;

namespace SIFS.Api.Admin
{
    [ApiController]
    [Route("api/admin/rbac")]
    [Authorize]
    public class RbacManagementController : ControllerBase
    {
        private readonly ICurrentService _currentService;
        private readonly IPermissionService _permissionService;

        public RbacManagementController(
            ICurrentService currentService,
            IPermissionService permissionService)
        {
            _currentService = currentService;
            _permissionService = permissionService;
        }

        [HttpGet("permissions/me")]
        [RequirePermission("admin:access")]
        public async Task<IActionResult> GetCurrentUserPermissions()
        {
            var result = await _permissionService.GetUserPermissionsAsync(_currentService.RequiredUuid);

            if (result.IsSuccess)
                return Ok(result.Data);

            return BadRequest(result.Message);
        }

        [HttpGet("roles/admin-check")]
        [RequireRole("admin")]
        public IActionResult CheckAdminRole()
        {
            return Ok(new { Role = "admin" });
        }
    }
}
