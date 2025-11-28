using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using FastPMS.Models.Domain;
using FastPMS.Models.ViewModel;
using FastPMS.Services;
using FastPMS.Data;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace FastPMS.Controllers
{
    [Authorize]
    public class TaskController : Controller
    {
        private readonly ITaskService _taskService;
        private readonly UserManager<Users> _userManager;
        private readonly PmsDbContext _context;

        public TaskController(ITaskService taskService, UserManager<Users> userManager, PmsDbContext context)
        {
            _taskService = taskService;
            _userManager = userManager;
            _context = context;
        }

        // GET: /Task/Create
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> Create()
        {
            await LoadViewData();
            return View();
        }

        // POST: /Task/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SuperAdmin,Admin,TeamMember")]
        public async Task<IActionResult> Create(CreateTaskRequest request)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var currentUser = await _userManager.GetUserAsync(User);
                    var task = await _taskService.CreateTaskAsync(request, currentUser.Id);

                    TempData["Success"] = $"Task '{request.Title}' created successfully!";
                    return RedirectToAction("Details", new { id = task.Id });
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Error creating task: {ex.Message}");
                }
            }

            await LoadViewData();
            return View(request);
        }

        // GET: /Task/MyTasks
        public async Task<IActionResult> MyTasks()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var tasks = await _taskService.GetTasksByAssignedUserAsync(currentUser.Id);
            return View(tasks);
        }

        // GET: /Task/Details/{id}
        public async Task<IActionResult> Details(string id)
        {
            var task = await _taskService.GetTaskByIdAsync(id);
            if (task == null)
            {
                return NotFound();
            }
            return View(task);
        }

        // GET: /Task/AllTasks
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> AllTasks(string priorityFilter = "", string statusFilter = "", string searchString = "")
        {
            var tasksQuery = _context.Tasks
                .Include(t => t.AssignedTo)
                .Include(t => t.Project)
                .Include(t => t.AssignedBy)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                tasksQuery = tasksQuery.Where(t =>
                    t.Title.Contains(searchString) ||
                    t.Description.Contains(searchString));
            }

            if (!string.IsNullOrEmpty(priorityFilter) && priorityFilter != "All")
            {
                tasksQuery = tasksQuery.Where(t => t.Priority == priorityFilter);
            }

            if (!string.IsNullOrEmpty(statusFilter) && statusFilter != "All")
            {
                tasksQuery = tasksQuery.Where(t => t.Status == statusFilter);
            }

            var tasks = await tasksQuery
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

            ViewBag.PriorityFilter = priorityFilter;
            ViewBag.StatusFilter = statusFilter;
            ViewBag.SearchString = searchString;
            ViewBag.Priorities = new List<string> { "All", "Low", "Medium", "High", "Urgent" };
            ViewBag.Statuses = new List<string> { "All", "Not Started", "In Progress", "Completed", "On Hold" };

            return View(tasks);
        }

        // ✅ ONLY ONE UpdateStatus ACTION - Remove any duplicates
        // POST: /Task/UpdateStatus
        [HttpPost]
        public async Task<JsonResult> UpdateStatus([FromBody] UpdateStatusModel model)
        {
            try
            {
                Console.WriteLine($"UpdateStatus called - TaskId: {model.TaskId}, Status: {model.Status}");

                if (string.IsNullOrEmpty(model.TaskId) || string.IsNullOrEmpty(model.Status))
                {
                    return Json(new { success = false, error = "Task ID and Status are required" });
                }

                // Use the simple version to avoid notification issues
                var success = await _taskService.UpdateTaskStatusSimpleAsync(model.TaskId, model.Status);

                if (success)
                {
                    Console.WriteLine("Status update successful");
                    return Json(new { success = true, message = "Status updated successfully" });
                }
                else
                {
                    Console.WriteLine("Status update failed in service");
                    return Json(new { success = false, error = "Failed to update status" });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Controller error: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return Json(new { success = false, error = ex.Message });
            }
        }

        // ✅ REMOVE ANY DUPLICATE UpdateStatus ACTIONS FROM HERE

        // GET: /Task/ProjectTasks/{projectId}
        public async Task<IActionResult> ProjectTasks(string projectId)
        {
            if (int.TryParse(projectId, out int projectIdInt))
            {
                var tasks = await _taskService.GetTasksByProjectAsync(projectIdInt);
                ViewBag.ProjectId = projectId;

                var project = await _context.Projects.FindAsync(projectIdInt);
                ViewBag.Project = project;

                return View(tasks);
            }
            else
            {
                TempData["ErrorMessage"] = "Invalid project ID";
                return View(new List<ProjectTask>());
            }
        }

        // GET: /Task/Edit/{id}
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> Edit(string id)
        {
            try
            {
                var task = await _taskService.GetTaskByIdAsync(id);
                if (task == null)
                {
                    TempData["Error"] = "Task not found";
                    return RedirectToAction("AllTasks");
                }

                await LoadViewData();

                var request = new CreateTaskRequest
                {
                    Title = task.Title,
                    Description = task.Description,
                    ProjectId = task.ProjectId,
                    AssignedToId = task.AssignedToId,
                    Priority = task.Priority,
                    DueDate = task.DueDate,
                    EstimatedHours = task.EstimatedHours
                };

                ViewBag.TaskId = id; // ✅ Add this for view reference
                return View(request);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error loading task: {ex.Message}";
                return RedirectToAction("AllTasks");
            }
        }

        // POST: /Task/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> Edit(string id, CreateTaskRequest request)
        {
            if (!ModelState.IsValid)
            {
                await LoadViewData();
                ViewBag.TaskId = id; // ✅ Add this
                return View(request);
            }

            try
            {
                var updatedTask = await _taskService.UpdateTaskAsync(id, request);
                if (updatedTask != null)
                {
                    TempData["Success"] = $"Task '{request.Title}' updated successfully!";
                    return RedirectToAction("AllTasks"); // ✅ Redirect to AllTasks instead of Details
                }
                else
                {
                    TempData["Error"] = "Task not found or update failed";
                    await LoadViewData();
                    ViewBag.TaskId = id;
                    return View(request);
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error updating task: {ex.Message}";
                await LoadViewData();
                ViewBag.TaskId = id;
                return View(request);
            }
        }

        // POST: /Task/Delete/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                var success = await _taskService.DeleteTaskAsync(id);
                if (success)
                {
                    TempData["Success"] = "Task deleted successfully!";
                }
                else
                {
                    TempData["Error"] = "Task not found or could not be deleted.";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error deleting task: {ex.Message}";
            }

            return RedirectToAction("AllTasks");
        }

        // UpdateStatus Model Class
        public class UpdateStatusModel
        {
            public string TaskId { get; set; }
            public string Status { get; set; }
        }

        private async Task LoadViewData()
        {
            ViewBag.Projects = await _context.Projects.ToListAsync();
            ViewBag.TeamMembers = await _context.Users
                .Where(u => u.Role != "Client")
                .ToListAsync();
            ViewBag.Priorities = new List<string> { "Low", "Medium", "High", "Urgent" };
        }
    }
}