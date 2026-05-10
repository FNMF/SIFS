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

        [HttpGet("roles")]
        [RequirePermission("admin:access")]
        public async Task<IActionResult> GetRoles()
        {
            var result = await _permissionService.GetRolesAsync();
            if (result.IsSuccess)
                return Ok(result.Data);

            return BadRequest(result.Message);
        }

        [HttpGet("me")]
        [RequirePermission("admin:access")]
        public async Task<IActionResult> GetCurrentUserRbac()
        {
            var userId = _currentService.RequiredUuid;
            var roles = await _permissionService.GetUserRolesAsync(userId);
            var permissions = await _permissionService.GetUserPermissionsAsync(userId);

            if (!roles.IsSuccess)
                return BadRequest(roles.Message);
            if (!permissions.IsSuccess)
                return BadRequest(permissions.Message);

            return Ok(new
            {
                user = new
                {
                    id = userId,
                    roles = roles.Data,
                    permissions = permissions.Data
                }
            });
        }

        [HttpGet("users/{id:guid}/roles")]
        [RequirePermission("admin:access")]
        public async Task<IActionResult> GetUserRoles([FromRoute] Guid id)
        {
            var result = await _permissionService.GetUserRolesAsync(id);
            if (result.IsSuccess)
                return Ok(result.Data);

            return BadRequest(result.Message);
        }

        [HttpPost("users/{id:guid}/roles")]
        [RequirePermission("admin:access")]
        public async Task<IActionResult> SetUserRoles([FromRoute] Guid id, [FromBody] SetUserRolesRequest? request)
        {
            var roleNames = request?.Roles ?? request?.RoleNames ?? new List<string>();
            var result = await _permissionService.SetUserRolesAsync(id, roleNames);
            if (!result.IsSuccess)
                return BadRequest(result.Message);

            var roles = await _permissionService.GetUserRolesAsync(id);
            var permissions = await _permissionService.GetUserPermissionsAsync(id);

            return Ok(new
            {
                roles = roles.Data ?? new List<string>(),
                permissions = permissions.Data ?? new List<string>()
            });
        }

        [HttpGet("users/{id:guid}/permissions")]
        [RequirePermission("admin:access")]
        public async Task<IActionResult> GetUserPermissions([FromRoute] Guid id)
        {
            var result = await _permissionService.GetUserPermissionsAsync(id);
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

    public class SetUserRolesRequest
    {
        public List<string>? Roles { get; set; }

        public List<string>? RoleNames { get; set; }
    }
}
