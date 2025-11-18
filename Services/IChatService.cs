using FastPMS.Models;
using FastPMS.Models.Domain;

namespace FastPMS.Services
{
    public interface IChatService
    {
        Task<List<Users>> GetChatUsersAsync(string currentUserId, string currentUserRole);
        Task<List<LiveChat>> GetChatHistoryAsync(string currentUserId, string otherUserId);
        Task SendMessageAsync(string senderId, string receiverId, string message);
        Task<Dictionary<string, int>> GetUnreadCountsAsync(string userId);
        Task<bool> CanSendMessageAsync(string senderRole, string receiverRole);
    }
}