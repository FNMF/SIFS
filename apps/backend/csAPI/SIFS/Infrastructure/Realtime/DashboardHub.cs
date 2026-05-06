using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using SIFS.Application.Rbac;

namespace SIFS.Infrastructure.Realtime
{
    [Authorize]
    public class DashboardHub : Hub
    {
        public const string AdminDashboardGroup = "admin-dashboard";
        private readonly IPermissionService _permissionService;

        public DashboardHub(IPermissionService permissionService)
        {
            _permissionService = permissionService;
        }

        public override async Task OnConnectedAsync()
        {
            var userIdText = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdText, out var userId))
            {
                Context.Abort();
                return;
            }

            var permissionResult = await _permissionService.HasPermissionAsync(userId, "admin:access");
            if (!permissionResult.IsSuccess || !permissionResult.Data)
            {
                Context.Abort();
                return;
            }

            await Groups.AddToGroupAsync(Context.ConnectionId, AdminDashboardGroup);
            await base.OnConnectedAsync();
        }
    }
}
