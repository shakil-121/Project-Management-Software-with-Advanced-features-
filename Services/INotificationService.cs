using FastPMS.Models.Domain;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FastPMS.Services
{
    public interface INotificationService
    {
        Task CreateNotificationAsync(string userId, string message, string type, string relatedId = null);
        Task<List<Notification>> GetUserNotificationsAsync(string userId);
        Task<List<Notification>> GetUnreadNotificationsAsync(string userId);
        Task MarkAsReadAsync(string notificationId);
        Task MarkAllAsReadAsync(string userId);
        Task<int> GetUnreadCountAsync(string userId);
        Task NotifyTaskAssignmentAsync(string userId, string v1, string v2);
        Task MarkAsReadAsync(int notificationId, string? userId);
    }
}