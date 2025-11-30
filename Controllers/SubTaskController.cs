using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using FastPMS.Models.Domain;
using FastPMS.Models.ViewModel;
using FastPMS.Services;
using FastPMS.Data;
using System.Security.Claims;

namespace FastPMS.Controllers
{
    [Authorize]
    public class SubTaskController : Controller
    {
        private readonly ISubTaskService _subTaskService;
        private readonly UserManager<Users> _userManager;
        private readonly PmsDbContext _context;

        public SubTaskController(
            ISubTaskService subTaskService,
            UserManager<Users> userManager,
            PmsDbContext context)
        {
            _subTaskService = subTaskService;
            _userManager = userManager;
            _context = context;
        }

        // GET: /SubTask/SubTaskCreate
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> SubTaskCreate()
        {
            await LoadViewData();
            var model = new SubTaskCreateViewModel
            {
                DueDate = DateTime.Now.AddDays(7)
            };
            return View(model);
        }

        // POST: /SubTask/SubTaskCreate - CLEAN FIXED VERSION
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> SubTaskCreate(SubTaskCreateViewModel model)
        {
            Console.WriteLine("=== SUBTASK CREATE POST STARTED ===");
            Console.WriteLine($"Title: {model.Title}, TaskId: {model.ProjectTaskId}, AssignTo: {model.AssignedToId}");

            if (ModelState.IsValid)
            {
                try
                {
                    Console.WriteLine("✅ ModelState is VALID");

                    // Get current user
                    var currentUser = await _userManager.GetUserAsync(User);
                    if (currentUser == null)
                    {
                        ModelState.AddModelError("", "User not found. Please login again.");
                        await LoadViewData();
                        return View(model);
                    }

                    Console.WriteLine($"Current User: {currentUser.Id} - {currentUser.UserName}");

                    // Create sub-task
                    var subTask = await _subTaskService.CreateSubTaskAsync(model, currentUser.Id);

                    Console.WriteLine($"✅ SubTask Created Successfully: {subTask.Id}");

                    TempData["SuccessMessage"] = $"Sub-task '{model.Title}' created successfully!";
                    return RedirectToAction("AllSubTasks");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ ERROR: {ex.Message}");
                    ModelState.AddModelError("", $"Error: {ex.Message}");
                    TempData["ErrorMessage"] = $"Failed to create sub-task: {ex.Message}";
                }
            }
            else
            {
                Console.WriteLine("❌ ModelState is INVALID");
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine($"Model Error: {error.ErrorMessage}");
                }
                TempData["ErrorMessage"] = "Please fix the validation errors below.";
            }

            await LoadViewData();
            return View(model);
        }
        // GET: /SubTask/AllSubTasks
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> AllSubTasks()
        {
            var subTasks = await _context.SubTasks
                .Include(st => st.ProjectTask)
                .Include(st => st.AssignedTo)
                .Include(st => st.AssignedBy)
                .OrderByDescending(st => st.CreatedAt)
                .ToListAsync();

            Console.WriteLine($"📊 Loaded {subTasks.Count} sub-tasks from database");

            return View(subTasks);
        }

        // GET: /SubTask/MySubTasks
        public async Task<IActionResult> MySubTasks()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var subTasks = await _subTaskService.GetUserSubTasksAsync(currentUser.Id);
            return View(subTasks);
        }

        // GET: /SubTask/SubTaskDetails/{id}
        public async Task<IActionResult> SubTaskDetails(string id)
        {
            var subTask = await _subTaskService.GetSubTaskByIdAsync(id);
            if (subTask == null)
            {
                TempData["ErrorMessage"] = "Sub-task not found";
                return RedirectToAction("AllSubTasks");
            }
            return View(subTask);
        }

        // Helper Methods
        private async Task LoadViewData()
        {
            try
            {
                // Load tasks
                ViewBag.AvailableTasks = await _context.Tasks
                    .Include(t => t.Project)
                    .Where(t => t.Status != "Completed" && t.Status != "Cancelled")
                    .OrderBy(t => t.Title)
                    .ToListAsync();

                // Load team members
                ViewBag.AvailableTeamMembers = await _context.Users
                    .Where(u => u.Role == "TeamMember" || u.Role == "Admin" || u.Role == "SuperAdmin")
                    .OrderBy(u => u.FullName)
                    .ToListAsync();

                Console.WriteLine($"📋 Loaded {ViewBag.AvailableTasks?.Count ?? 0} tasks");
                Console.WriteLine($"👥 Loaded {ViewBag.AvailableTeamMembers?.Count ?? 0} team members");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error loading view data: {ex.Message}");
                ViewBag.AvailableTasks = new List<ProjectTask>();
                ViewBag.AvailableTeamMembers = new List<Users>();
            }
        }
    }

    public class UpdateSubTaskStatusModel
    {
        public string SubTaskId { get; set; }
        public string Status { get; set; }
    }
}