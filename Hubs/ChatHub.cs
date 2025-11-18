using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using FastPMS.Models.Domain;
using FastPMS.Services;
using Microsoft.Extensions.Logging;

namespace FastPMS.Hubs
{
    public class ChatHub : Hub
    {
        private readonly UserManager<Users> _userManager;
        private readonly IChatService _chatService;
        private readonly ILogger<ChatHub> _logger;

        public ChatHub(UserManager<Users> userManager, IChatService chatService, ILogger<ChatHub> logger)
        {
            _userManager = userManager;
            _chatService = chatService;
            _logger = logger;
        }

        public override async Task OnConnectedAsync()
        {
            var user = await _userManager.GetUserAsync(Context.User);
            if (user != null)
            {
                // User কে তাদের নিজের ID তে group এ add করুন
                await Groups.AddToGroupAsync(Context.ConnectionId, user.Id);
                _logger.LogInformation($"✅ USER CONNECTED: {user.UserName} ({user.Id}) - Connection: {Context.ConnectionId}");

                // Online status broadcast করুন
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
                _logger.LogInformation($"❌ USER DISCONNECTED: {user.UserName}");
            }
            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendMessage(string receiverId, string message)
        {
            try
            {
                var sender = await _userManager.GetUserAsync(Context.User);
                if (sender == null)
                {
                    _logger.LogError("❌ SendMessage: Sender not found");
                    return;
                }

                _logger.LogInformation($"✉️ MESSAGE SEND: {sender.UserName} -> {receiverId}: {message}");

                // 1. Database এ message save করুন
                await _chatService.SendMessageAsync(sender.Id, receiverId, message);
                _logger.LogInformation("✅ Message saved to database");

                // 2. Sender এর role get করুন
                var senderRoles = await _userManager.GetRolesAsync(sender);
                var senderRole = senderRoles.FirstOrDefault();

                // 3. Message object তৈরি করুন
                var messageObj = new
                {
                    SenderId = sender.Id,
                    SenderName = sender.UserName,
                    SenderRole = senderRole,
                    Message = message,
                    Timestamp = DateTime.Now
                };

                _logger.LogInformation($"📤 Sending to RECEIVER's group: {receiverId}");

                // 4. Receiver কে message send করুন (GROUP ব্যবহার করে)
                await Clients.Group(receiverId).SendAsync("ReceiveMessage", messageObj);
                _logger.LogInformation($"✅ Message sent to receiver's group: {receiverId}");

                // 5. Sender কেও message send করুন (ইমিডিয়েট UI update এর জন্য)
                await Clients.Caller.SendAsync("ReceiveMessage", messageObj);
                _logger.LogInformation($"✅ Message sent back to sender: {sender.UserName}");

            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ ERROR in SendMessage: {ex.Message}");
                throw;
            }
        }

        // Connection test method
        public async Task<string> TestConnection(string message)
        {
            var user = await _userManager.GetUserAsync(Context.User);
            return $"✅ Connection working for {user?.UserName}. Test message: {message}";
        }

        public async Task JoinUserGroup()
        {
            var user = await _userManager.GetUserAsync(Context.User);
            if (user != null)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, user.Id);
                _logger.LogInformation($"✅ User {user.UserName} joined their group: {user.Id}");
                await Clients.Caller.SendAsync("GroupJoined", $"Joined group: {user.Id}");
            }
        }

        public async Task Typing(string receiverId)
        {
            var sender = await _userManager.GetUserAsync(Context.User);
            if (sender != null)
            {
                await Clients.Group(receiverId).SendAsync("UserTyping", sender.Id, sender.UserName);
            }
        }

        public async Task StopTyping(string receiverId)
        {
            var sender = await _userManager.GetUserAsync(Context.User);
            if (sender != null)
            {
                await Clients.Group(receiverId).SendAsync("UserStoppedTyping", sender.Id);
            }
        }
    }
}