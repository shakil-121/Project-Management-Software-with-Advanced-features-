using FastPMS.Data;
using FastPMS.Models;
using FastPMS.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;

namespace FastPMS.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly PmsDbContext pmsDbContext;
        private readonly INotificationService _notificationService;

        public HomeController(ILogger<HomeController> logger, PmsDbContext pmsDbContext, INotificationService notificationService)
        {
            _logger = logger;
            this.pmsDbContext = pmsDbContext;
            _notificationService = notificationService;
        }

        public IActionResult Index()
        {
            var projects = pmsDbContext.Projects.ToList();

            var notStartedCount = projects.Count(p => p.Status.Trim().ToLower() == "not started");
            var inProgressCount = projects.Count(p => p.Status.Trim().ToLower() == "in progress");
            var onHoldCount = projects.Count(p => p.Status.Trim().ToLower() == "on hold");
            var CompletedCount = projects.Count(p => p.Status == "Completed");

            // Total count of all projects
            var totalProjectsCount = projects.Count();

            Console.WriteLine($"Completed Projects Count: {CompletedCount}");

            // Pass the counts to the view using ViewBag
            ViewBag.NotStartedCount = notStartedCount;
            ViewBag.InProgressCount = inProgressCount;
            ViewBag.OnHoldCount = onHoldCount;
            ViewBag.CompletedCount = CompletedCount;
            ViewBag.TotalProjectsCount = totalProjectsCount;

            return View();
        }

        // ? Test Notification Method
        [Authorize]
        public async Task<IActionResult> TestNotification()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!string.IsNullOrEmpty(userId))
            {
                // Test different notification types
                await _notificationService.CreateNotificationAsync(userId, "?? Welcome to FastPMS!", "success");
                await _notificationService.CreateNotificationAsync(userId, "?? New task 'Design Dashboard' assigned to you", "task");
                await _notificationService.CreateNotificationAsync(userId, "?? Project 'FastPMS Development' has started", "project");
                await _notificationService.CreateNotificationAsync(userId, "?? You have a new message from Team Lead", "message");

                TempData["Success"] = "Test notifications sent! Check your notification bell.";
            }

            return RedirectToAction("Index");
        }

        // ? Project Notification Method
        [Authorize]
        public async Task<IActionResult> SendProjectNotification()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!string.IsNullOrEmpty(userId))
            {
                await _notificationService.CreateNotificationAsync(userId, "?? New project milestone achieved!", "success");
                TempData["Success"] = "Project notification sent!";
            }

            return RedirectToAction("Index");
        }

        // ? Task Assignment Notification
        [Authorize]
        public async Task<IActionResult> TestTaskAssignment()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!string.IsNullOrEmpty(userId))
            {
                await _notificationService.NotifyTaskAssignmentAsync(userId, "Design Homepage", "E-commerce Website");
                TempData["Success"] = "Task assignment notification sent!";
            }

            return RedirectToAction("Index");
        }

        // ? Auto Send Welcome Notification
        [Authorize]
        public async Task<IActionResult> SendWelcomeNotification()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userName = User.Identity.Name;

            if (!string.IsNullOrEmpty(userId))
            {
                await _notificationService.CreateNotificationAsync(userId, $"?? Welcome {userName}! FastPMS is ready to use.", "success");
                TempData["Success"] = "Welcome notification sent!";
            }

            return RedirectToAction("Index");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}