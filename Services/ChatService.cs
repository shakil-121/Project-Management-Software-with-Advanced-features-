using FastPMS.Models;
using FastPMS.Repositories.Interfaces;
using Microsoft.AspNetCore.Identity;
using FastPMS.Models.Domain;

namespace FastPMS.Services
{
    public class ChatService : IChatService
    {
        private readonly IChatRepository _chatRepository;
        private readonly UserManager<Users> _userManager;

        public ChatService(IChatRepository chatRepository, UserManager<Users> userManager)
        {
            _chatRepository = chatRepository;
            _userManager = userManager;
        }

        public async Task<List<Users>> GetChatUsersAsync(string currentUserId, string currentUserRole)
        {
            return await _chatRepository.GetAvailableUsersAsync(currentUserId, currentUserRole);
        }

        public async Task<List<LiveChat>> GetChatHistoryAsync(string currentUserId, string otherUserId)
        {
            return await _chatRepository.GetConversationAsync(currentUserId, otherUserId);
        }

        public async Task SendMessageAsync(string senderId, string receiverId, string message)
        {
            await _chatRepository.SendMessageAsync(senderId, receiverId, message);
        }

        public async Task<Dictionary<string, int>> GetUnreadCountsAsync(string userId)
        {
            var unreadCount = await _chatRepository.GetUnreadCountAsync(userId);
            return new Dictionary<string, int> { { "total", unreadCount } };
        }

        public async Task<bool> CanSendMessageAsync(string senderRole, string receiverRole)
        {
            return (senderRole, receiverRole) switch
            {
                ("Client", "SuperAdmin") => true,
                ("TeamMember", "Admin") => true,
                ("TeamMember", "SuperAdmin") => true,
                ("TeamMember", "TeamMember") => true,
                ("Admin", "SuperAdmin") => true,
                ("Admin", "TeamMember") => true,
                ("SuperAdmin", "Client") => true,
                ("SuperAdmin", "TeamMember") => true,
                ("SuperAdmin", "Admin") => true,
                ("SuperAdmin", "SuperAdmin") => true,
                _ => false
            };
        }
    }
}