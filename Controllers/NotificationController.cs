using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using FastPMS.Services;
using System.Linq;
using System.Security.Claims;

namespace FastPMS.Controllers
{
    [Authorize]
    public class NotificationController : Controller
    {
        private readonly INotificationService _notificationService;

        public NotificationController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        [HttpGet]
        public async Task<IActionResult> GetNotifications()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { notifications = new object[0], unreadCount = 0 });
            }

            var notifications = await _notificationService.GetUserNotificationsAsync(userId);
            var unreadCount = await _notificationService.GetUnreadCountAsync(userId);

            return Json(new
            {
                notifications = notifications.Select(n => new {
                    n.Id,
                    n.Message,
                    n.Type,
                    n.IsRead,
                    n.CreatedAt
                }),
                unreadCount
            });
        }

        [HttpPost]
        public async Task<IActionResult> MarkAsRead([FromBody] MarkAsReadRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            await _notificationService.MarkAsReadAsync(request.NotificationId, userId);
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            await _notificationService.MarkAllAsReadAsync(userId);
            return Ok();
        }

        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var notifications = await _notificationService.GetUserNotificationsAsync(userId);
            return View(notifications);
        }
    }

    public class MarkAsReadRequest
    {
        public int NotificationId { get; set; }
    }
}