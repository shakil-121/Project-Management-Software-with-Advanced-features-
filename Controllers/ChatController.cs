using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using FastPMS.Models.Domain;
using FastPMS.Services;
using System.Security.Claims;

namespace FastPMS.Controllers
{
    [Authorize]
    public class ChatController : Controller
    {
        private readonly IChatService _chatService;
        private readonly UserManager<Users> _userManager;

        public ChatController(IChatService chatService, UserManager<Users> userManager)
        {
            _chatService = chatService;
            _userManager = userManager;
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

                await _chatService.SendMessageAsync(currentUser.Id, receiverId, message);

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<JsonResult> GetUnreadCount()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var unreadCounts = await _chatService.GetUnreadCountsAsync(currentUser.Id);
            return Json(unreadCounts);
        }
    }
}