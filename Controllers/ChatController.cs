using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using FastPMS.Models.Domain;
using FastPMS.Services;
using System.Security.Claims;
using System.Threading.Tasks;

namespace FastPMS.Controllers
{
    [Authorize]
    public class ChatController : Controller
    {
        private readonly IChatService _chatService;
        private readonly UserManager<Users> _userManager;
        private readonly INotificationService _notificationService;

        public ChatController(IChatService chatService, UserManager<Users> userManager, INotificationService notificationService)
        {
            _chatService = chatService;
            _userManager = userManager;
            _notificationService = notificationService;
        }

        public async Task<IActionResult> Index()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value ?? "User";

            var chatUsers = await _chatService.GetChatUsersAsync(currentUser.Id, currentUserRole);
            ViewBag.CurrentUserRole = currentUserRole;

            return View(chatUsers);
        }

        [HttpGet]
        public async Task<JsonResult> GetChatHistory(string otherUserId)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var chatHistory = await _chatService.GetChatHistoryAsync(currentUser.Id, otherUserId);

            return Json(chatHistory);
        }

        [HttpPost]
        public async Task<JsonResult> SendMessage(string receiverId, string message)
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                var receiver = await _userManager.FindByIdAsync(receiverId);

                if (receiver == null)
                    return Json(new { success = false, error = "User not found" });

                var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value ?? "User";
                var receiverRole = (await _userManager.GetRolesAsync(receiver)).FirstOrDefault();

                if (!await _chatService.CanSendMessageAsync(currentUserRole, receiverRole))
                    return Json(new { success = false, error = "You don't have permission to message this user" });

                // Send message
                await _chatService.SendMessageAsync(currentUser.Id, receiverId, message);

                // Create notification for receiver
                await _notificationService.CreateNotificationAsync(
                    receiverId,
                    $"New message from {currentUser.FullName ?? currentUser.UserName}",
                    "message",
                    currentUser.Id // RelatedId as sender's ID
                );

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        // 🔔 NEW NOTIFICATION METHODS

        [HttpGet]
        public async Task<JsonResult> GetNotifications()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var notifications = await _notificationService.GetUserNotificationsAsync(currentUser.Id);

            return Json(notifications);
        }

        [HttpGet]
        public async Task<JsonResult> GetUnreadNotificationCount()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var count = await _notificationService.GetUnreadCountAsync(currentUser.Id);

            return Json(new { count });
        }

        [HttpPost]
        public async Task<JsonResult> MarkNotificationAsRead(string notificationId)
        {
            await _notificationService.MarkAsReadAsync(notificationId);
            return Json(new { success = true });
        }

        [HttpPost]
        public async Task<JsonResult> MarkAllNotificationsAsRead()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            await _notificationService.MarkAllAsReadAsync(currentUser.Id);
            return Json(new { success = true });
        }

        [HttpPost]
        public async Task<IActionResult> TestNotification()
        {
            var currentUser = await _userManager.GetUserAsync(User);

            await _notificationService.CreateNotificationAsync(
                currentUser.Id,
                "Test notification from Chat Controller! ✅",
                "test",
                currentUser.Id
            );

            TempData["Message"] = "Test notification created! Check the bell icon.";
            return RedirectToAction("Index");
        }
    }
}