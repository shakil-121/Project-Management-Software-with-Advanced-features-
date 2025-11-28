using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace FastPMS.Hubs
{
    public class NotificationHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            // User er ID ta connection er sathe associate kora
            var userId = Context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"user-{userId}");
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var userId = Context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user-{userId}");
            }

            await base.OnDisconnectedAsync(exception);
        }

        // Test method
        public async Task SendTestNotification(string userId, string message)
        {
            await Clients.Group($"user-{userId}").SendAsync("ReceiveNotification", new
            {
                message = message,
                type = "test",
                createdAt = DateTime.Now
            });
        }
    }
}