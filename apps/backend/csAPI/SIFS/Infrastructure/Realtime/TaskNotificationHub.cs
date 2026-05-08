using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace SIFS.Infrastructure.Realtime
{
    [Authorize]
    public class TaskNotificationHub : Hub
    {
        public static string UserGroup(Guid userId) => $"user-task-notifications:{userId:N}";

        public override async Task OnConnectedAsync()
        {
            var userIdText = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdText, out var userId))
            {
                Context.Abort();
                return;
            }

            await Groups.AddToGroupAsync(Context.ConnectionId, UserGroup(userId));
            await base.OnConnectedAsync();
        }
    }
}
