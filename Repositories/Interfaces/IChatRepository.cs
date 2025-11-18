using FastPMS.Models;
using FastPMS.Models.Domain;

namespace FastPMS.Repositories.Interfaces
{
    public interface IChatRepository
    {
        Task<List<LiveChat>> GetConversationAsync(string currentUserId, string otherUserId);
        Task<List<Users>> GetAvailableUsersAsync(string currentUserId, string currentUserRole);
        Task SendMessageAsync(string senderId, string receiverId, string message);
        Task MarkAsReadAsync(int messageId);
        Task<int> GetUnreadCountAsync(string userId);
    }
}