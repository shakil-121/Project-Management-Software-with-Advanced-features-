using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using FastPMS.Data;
using FastPMS.Models.Domain;
using FastPMS.Hubs;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FastPMS.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly PmsDbContext _context;

        public NotificationService(IHubContext<NotificationHub> hubContext, PmsDbContext context)
        {
            _hubContext = hubContext;
            _context = context;
        }

        public async Task CreateNotificationAsync(string userId, string message, string type, string relatedId = null)
        {
            // 1. Database e save
            var notification = new Notification
            {
                UserId = userId,
                Message = message,
                Type = type,
                RelatedId = relatedId ?? string.Empty, // NULL avoid
                IsRead = false,
                CreatedAt = DateTime.Now
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            // 2. Real-time send via SignalR
            await _hubContext.Clients.Group($"user-{userId}").SendAsync("ReceiveNotification", new
            {
                id = notification.Id,
                message = notification.Message,
                type = notification.Type,
                relatedId = notification.RelatedId,
                createdAt = notification.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"),
                isRead = notification.IsRead
            });
        }

        public async Task<List<Notification>> GetUserNotificationsAsync(string userId)
        {
            return await _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .Take(50) // Last 50 notifications
                .ToListAsync();
        }

        public async Task<List<Notification>> GetUnreadNotificationsAsync(string userId)
        {
            return await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        public async Task MarkAsReadAsync(string notificationId)
        {
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == notificationId);

            if (notification != null)
            {
                notification.IsRead = true;
                await _context.SaveChangesAsync();
            }
        }

        public async Task MarkAllAsReadAsync(string userId)
        {
            var unreadNotifications = await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToListAsync();

            foreach (var notification in unreadNotifications)
            {
                notification.IsRead = true;
            }

            await _context.SaveChangesAsync();
        }

        public async Task<int> GetUnreadCountAsync(string userId)
        {
            return await _context.Notifications
                .CountAsync(n => n.UserId == userId && !n.IsRead);
        }

        public Task NotifyTaskAssignmentAsync(string userId, string v1, string v2)
        {
            throw new NotImplementedException();
        }

        public Task MarkAsReadAsync(int notificationId, string? userId)
        {
            throw new NotImplementedException();
        }
    }
}