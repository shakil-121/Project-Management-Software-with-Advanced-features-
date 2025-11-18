using FastPMS.Data;
using FastPMS.Models;
using FastPMS.Models.Domain;
using FastPMS.Repositories.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FastPMS.Repositories
{
    public class ChatRepository : IChatRepository
    {
        private readonly PmsDbContext _context;
        private readonly UserManager<Users> _userManager;

        public ChatRepository(PmsDbContext context, UserManager<Users> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<List<LiveChat>> GetConversationAsync(string currentUserId, string otherUserId)
        {
            return await _context.LiveChats
                .Where(m => (m.SenderId == currentUserId && m.ReceiverId == otherUserId) ||
                           (m.SenderId == otherUserId && m.ReceiverId == currentUserId))
                .OrderBy(m => m.Timestamp)
                .Include(m => m.Sender)
                .Include(m => m.Receiver)
                .ToListAsync();
        }

        public async Task<List<Users>> GetAvailableUsersAsync(string currentUserId, string currentUserRole)
        {
            var allUsers = await _userManager.Users
                .Where(u => u.Id != currentUserId)
                .ToListAsync();

            var availableUsers = new List<Users>();

            foreach (var user in allUsers)
            {
                var userRoles = await _userManager.GetRolesAsync(user);
                var userRole = userRoles.FirstOrDefault();

                if (CanMessageBasedOnRoles(currentUserRole, userRole))
                {
                    availableUsers.Add(user);
                }
            }

            return availableUsers;
        }

        private bool CanMessageBasedOnRoles(string senderRole, string receiverRole)
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

        public async Task SendMessageAsync(string senderId, string receiverId, string message)
        {
            var chatMessage = new LiveChat
            {
                SenderId = senderId,
                ReceiverId = receiverId,
                Message = message,
                Timestamp = DateTime.Now,
                IsRead = false
            };

            _context.LiveChats.Add(chatMessage);
            await _context.SaveChangesAsync();
        }

        public async Task MarkAsReadAsync(int messageId)
        {
            var message = await _context.LiveChats.FindAsync(messageId);
            if (message != null)
            {
                message.IsRead = true;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<int> GetUnreadCountAsync(string userId)
        {
            return await _context.LiveChats
                .CountAsync(m => m.ReceiverId == userId && !m.IsRead);
        }
    }
}