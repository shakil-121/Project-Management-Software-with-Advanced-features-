using FastPMS.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FastPMS.Controllers
{
    [Authorize(Roles = "Client")]
    public class ProjectController : Controller
    {
        private readonly IProjectRepository _projectRepository;

        public ProjectController(IProjectRepository projectRepository)
        {
            _projectRepository = projectRepository;
        }

        // ✅ GET: Client Dashboard - My Projects
        [HttpGet]
        public async Task<IActionResult> MyProjectStatus()
        {
            try
            {
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                // শুধুমাত্র এই Client-এর assigned projects
                var clientProjects = await _projectRepository.GetProjectsByClientAsync(currentUserId);

                // Statistics for dashboard
                var totalProjects = clientProjects.Count;
                var completedProjects = clientProjects.Count(p => p.Status == "Completed");
                var inProgressProjects = clientProjects.Count(p => p.Status == "In Progress");
                var notStartedProjects = clientProjects.Count(p => p.Status == "Not Started");
                var onHoldProjects = clientProjects.Count(p => p.Status == "On Hold");

                ViewBag.TotalProjects = totalProjects;
                ViewBag.CompletedProjects = completedProjects;
                ViewBag.InProgressProjects = inProgressProjects;
                ViewBag.NotStartedProjects = notStartedProjects;
                ViewBag.OnHoldProjects = onHoldProjects;

                return View(clientProjects);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error loading your projects: {ex.Message}";
                return View(new List<FastPMS.Models.Domain.Project>());
            }
        }

        // ✅ GET: Project Details for Client
        [HttpGet]
        public async Task<IActionResult> ProjectDetails(int id)
        {
            try
            {
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                // Verify that this project belongs to the current client
                var clientProjects = await _projectRepository.GetProjectsByClientAsync(currentUserId);
                var project = clientProjects.FirstOrDefault(p => p.ProjectId == id);

                if (project == null)
                {
                    TempData["ErrorMessage"] = "Project not found or you don't have access to this project!";
                    return RedirectToAction("MyProjectStatus");
                }

                return View(project);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error loading project details: {ex.Message}";
                return RedirectToAction("MyProjectStatus");
            }
        }

        // ✅ GET: Client Profile
        [HttpGet]
        public async Task<IActionResult> MyProfile()
        {
            try
            {
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var user = await _projectRepository.GetAllClientsAsync();
                var currentUser = user.FirstOrDefault(u => u.Id == currentUserId);

                if (currentUser == null)
                {
                    TempData["ErrorMessage"] = "User not found!";
                    return RedirectToAction("MyProjectStatus");
                }

                return View(currentUser);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error loading profile: {ex.Message}";
                return RedirectToAction("MyProjectStatus");
            }
        }
    }
}