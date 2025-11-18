using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using FastPMS.Models.Domain;
using FastPMS.Services;

namespace FastPMS.Hubs
{
    public class ChatHub : Hub
    {
        private readonly UserManager<Users> _userManager;
        private readonly IChatService _chatService;

        public ChatHub(UserManager<Users> userManager, IChatService chatService)
        {
            _userManager = userManager;
            _chatService = chatService;
        }

        public override async Task OnConnectedAsync()
        {
            var user = await _userManager.GetUserAsync(Context.User);
            if (user != null)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, user.Id);
                await Clients.Others.SendAsync("UserOnline", user.Id, user.UserName);
            }
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var user = await _userManager.GetUserAsync(Context.User);
            if (user != null)
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, user.Id);
                await Clients.Others.SendAsync("UserOffline", user.Id);
            }
            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendMessage(string receiverId, string message)
        {
            var sender = await _userManager.GetUserAsync(Context.User);
            if (sender == null) return;

            await _chatService.SendMessageAsync(sender.Id, receiverId, message);

            // Get sender role
            var senderRoles = await _userManager.GetRolesAsync(sender);
            var senderRole = senderRoles.FirstOrDefault();

            // Send to receiver
            await Clients.User(receiverId).SendAsync("ReceiveMessage", new
            {
                SenderId = sender.Id,
                SenderName = sender.UserName,
                SenderRole = senderRole,
                Message = message, 
                Timestamp = DateTime.Now
            });

            // Send back to sender
            await Clients.Caller.SendAsync("ReceiveMessage", new
            {
                SenderId = sender.Id,
                SenderName = sender.UserName,
                SenderRole = senderRole,
                Message = message, 
                Timestamp = DateTime.Now
            });
        }

        public async Task Typing(string receiverId)
        {
            var sender = await _userManager.GetUserAsync(Context.User);
            if (sender != null)
            {
                await Clients.User(receiverId).SendAsync("UserTyping", sender.Id, sender.UserName);
            }
        }

        public async Task StopTyping(string receiverId)
        {
            var sender = await _userManager.GetUserAsync(Context.User);
            if (sender != null)
            {
                await Clients.User(receiverId).SendAsync("UserStoppedTyping", sender.Id);
            }
        }
    }
}