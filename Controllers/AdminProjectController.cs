using FastPMS.Data;
using FastPMS.Models.Domain;
using FastPMS.Models.ViewModel;
using FastPMS.Repositories;
using FastPMS.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using System.IO;
using System.Linq;
using System.Security.Claims;

namespace FastPMS.Controllers
{
    [Authorize(Roles = "SuperAdmin,Admin")]
    public class AdminProjectController : Controller
    {
        private readonly IProjectRepository _projectRepository;
        private readonly IDeveloperRepository _developerRepository;
        private readonly UserManager<Users> _userManager;
        private readonly PmsDbContext _context;

        // ✅ Updated Constructor with all dependencies
        public AdminProjectController(
            IProjectRepository projectRepository,
            IDeveloperRepository developerRepository,
            UserManager<Users> userManager,
            PmsDbContext context)
        {
            _projectRepository = projectRepository;
            _developerRepository = developerRepository;
            _userManager = userManager;
            _context = context;
        }

        // ✅ GET: Create Project with Client List
        [HttpGet]
        public async Task<IActionResult> CreateProject()
        {
            try
            {
                await LoadClients();
                return View();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error loading clients: {ex.Message}";
                return View();
            }
        }

        // ✅ POST: Create Project with Client Assignment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateProject(CreateProjectRequest createProjectRequest, List<string> SelectedClientIds)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Create Project
                    var project = new Project
                    {
                        ProjectTitle = createProjectRequest.ProjectTitle,
                        ProjectDescription = createProjectRequest.ProjectDescription,
                        Stack = createProjectRequest.Stack,
                        StartDate = createProjectRequest.StartDate,
                        EndDate = createProjectRequest.EndDate,
                        Status = createProjectRequest.Status,
                    };

                    // Save Project
                    var createdProject = await _projectRepository.AddProjectAsync(project);

                    // Assign Clients if any selected
                    if (SelectedClientIds != null && SelectedClientIds.Any())
                    {
                        var currentUser = await _userManager.GetUserAsync(User);
                        await _projectRepository.AssignClientsToProjectAsync(
                            createdProject.ProjectId,
                            SelectedClientIds,
                            currentUser?.FullName ?? "Admin"
                        );

                        TempData["SuccessMessage"] = $"Project '{createdProject.ProjectTitle}' created successfully and assigned to {SelectedClientIds.Count} client(s)!";
                    }
                    else
                    {
                        TempData["SuccessMessage"] = $"Project '{createdProject.ProjectTitle}' created successfully!";
                    }

                    return RedirectToAction("AllProject", "AdminProject");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Error creating project: {ex.Message}");
                    TempData["ErrorMessage"] = $"Error creating project: {ex.Message}";
                }
            }

            // Reload clients if validation fails
            await LoadClients();
            return View(createProjectRequest);
        }

        // ✅ GET: All Projects with Assigned Clients
        [HttpGet]
        public async Task<IActionResult> AllProject()
        {
            try
            {
                var projects = await _projectRepository.GetAllProjectsAsync();

                // Load assigned clients for each project
                foreach (var project in projects)
                {
                    var assignedClients = await _projectRepository.GetAssignedClientsAsync(project.ProjectId);
                    ViewData[$"AssignedClients_{project.ProjectId}"] = assignedClients;
                }

                // Statistics
                var notStartedCount = projects.Count(p => p.Status.Trim().ToLower() == "not started");
                var inProgressCount = projects.Count(p => p.Status.Trim().ToLower() == "in progress");
                var onHoldCount = projects.Count(p => p.Status.Trim().ToLower() == "on hold");
                var completedCount = projects.Count(p => p.Status == "Completed");
                var totalProjectsCount = projects.Count();

                ViewBag.NotStartedCount = notStartedCount;
                ViewBag.InProgressCount = inProgressCount;
                ViewBag.OnHoldCount = onHoldCount;
                ViewBag.CompletedCount = completedCount;
                ViewBag.TotalProjectsCount = totalProjectsCount;

                return View(projects);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error loading projects: {ex.Message}";
                return View(new List<Project>());
            }
        }

        // ✅ GET: Edit Project
        [HttpGet]
        public async Task<IActionResult> EditProject(int id)
        {
            try
            {
                var project = await _projectRepository.GetProjectById(id);
                if (project != null)
                {
                    var editProjectRequest = new EditProjectRequest
                    {
                        ProjectId = project.ProjectId,
                        ProjectTitle = project.ProjectTitle,
                        ProjectDescription = project.ProjectDescription,
                        Stack = project.Stack,
                        StartDate = project.StartDate,
                        EndDate = project.EndDate,
                        Status = project.Status,
                    };
                    return View(editProjectRequest);
                }

                TempData["ErrorMessage"] = "Project not found!";
                return RedirectToAction("AllProject");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error loading project: {ex.Message}";
                return RedirectToAction("AllProject");
            }
        }

        // ✅ POST: Edit Project
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProject(EditProjectRequest editProjectRequest)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var updatedProject = await _projectRepository.UpdateProjectAsync(editProjectRequest.ProjectId, editProjectRequest);

                    if (updatedProject != null)
                    {
                        TempData["SuccessMessage"] = $"Project '{updatedProject.ProjectTitle}' updated successfully!";
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Project not found!";
                    }
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = $"Error updating project: {ex.Message}";
                }
            }
            else
            {
                TempData["ErrorMessage"] = "Please correct the validation errors.";
            }

            return RedirectToAction("AllProject");
        }

        // ✅ POST: Delete Project
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteProject(int id)
        {
            try
            {
                var project = await _projectRepository.GetProjectById(id);
                if (project != null)
                {
                    await _projectRepository.DeleteProjectAsync(id);
                    TempData["SuccessMessage"] = $"Project '{project.ProjectTitle}' deleted successfully!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Project not found!";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error deleting project: {ex.Message}";
            }

            return RedirectToAction("AllProject");
        }

        // ✅ GET: Export to Excel
        [HttpGet]
        public async Task<IActionResult> ExportToExcel()
        {
            try
            {
                var excelData = await _projectRepository.ExportProjectsToExcelAsync();
                return File(excelData, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Projects_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error exporting to Excel: {ex.Message}";
                return RedirectToAction("AllProject");
            }
        }

        // ✅ GET: Project Details with Assigned Clients
        [HttpGet]
        public async Task<IActionResult> ProjectDetails(int id)
        {
            try
            {
                var project = await _projectRepository.GetProjectById(id);
                if (project == null)
                {
                    TempData["ErrorMessage"] = "Project not found!";
                    return RedirectToAction("AllProject");
                }

                var assignedClients = await _projectRepository.GetAssignedClientsAsync(id);
                ViewBag.AssignedClients = assignedClients;

                return View(project);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error loading project details: {ex.Message}";
                return RedirectToAction("AllProject");
            }
        }

        // ✅ GET: Manage Project Clients
        [HttpGet]
        public async Task<IActionResult> ManageProjectClients(int id)
        {
            try
            {
                var project = await _projectRepository.GetProjectById(id);
                if (project == null)
                {
                    TempData["ErrorMessage"] = "Project not found!";
                    return RedirectToAction("AllProject");
                }

                var assignedClients = await _projectRepository.GetAssignedClientsAsync(id);
                var allClients = await _projectRepository.GetAllClientsAsync();

                ViewBag.Project = project;
                ViewBag.AssignedClients = assignedClients;
                ViewBag.AllClients = allClients.Select(c => new SelectListItem
                {
                    Value = c.Id,
                    Text = $"{c.FullName} - {c.Email}",
                    Selected = assignedClients.Any(ac => ac.Id == c.Id)
                }).ToList();

                return View();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error loading client management: {ex.Message}";
                return RedirectToAction("AllProject");
            }
        }

        // ✅ POST: Update Project Clients
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProjectClients(int projectId, List<string> SelectedClientIds)
        {
            try
            {
                var project = await _projectRepository.GetProjectById(projectId);
                if (project == null)
                {
                    TempData["ErrorMessage"] = "Project not found!";
                    return RedirectToAction("AllProject");
                }

                // Remove existing assignments
                var existingAssignments = _context.ProjectUsers.Where(pu => pu.ProjectId == projectId);
                _context.ProjectUsers.RemoveRange(existingAssignments);
                await _context.SaveChangesAsync();

                // Add new assignments
                if (SelectedClientIds != null && SelectedClientIds.Any())
                {
                    var currentUser = await _userManager.GetUserAsync(User);
                    await _projectRepository.AssignClientsToProjectAsync(
                        projectId,
                        SelectedClientIds,
                        currentUser?.FullName ?? "Admin"
                    );

                    TempData["SuccessMessage"] = $"Project clients updated successfully! {SelectedClientIds.Count} client(s) assigned.";
                }
                else
                {
                    TempData["SuccessMessage"] = "All clients removed from project successfully!";
                }

                return RedirectToAction("AllProject");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error updating project clients: {ex.Message}";
                return RedirectToAction("AllProject");
            }
        }

        // ✅ GET: Add Developer
        [HttpGet]
        public IActionResult AddDev()
        {
            return View();
        }

        // ✅ POST: Add Developer
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddDev(Developer developer)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (developer.ImageFile != null)
                    {
                        using (var ms = new MemoryStream())
                        {
                            await developer.ImageFile.CopyToAsync(ms);
                            developer.image = ms.ToArray();
                        }
                    }

                    await _developerRepository.AddDeveloperAsync(developer);
                    TempData["SuccessMessage"] = "Developer added successfully!";
                    return RedirectToAction("AllDeveloper", "AdminProject");
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = $"Error adding developer: {ex.Message}";
                }
            }
            else
            {
                TempData["ErrorMessage"] = "Please correct the validation errors.";
            }

            return View(developer);
        }

        // ✅ GET: All Developers
        [HttpGet]
        public async Task<IActionResult> AllDeveloper()
        {
            try
            {
                var developers = await _developerRepository.GetAllDevelopersAsync();
                return View(developers);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error loading developers: {ex.Message}";
                return View(new List<Developer>());
            }
        }

        // ✅ POST: Delete Developer
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DevDelete(int id)
        {
            try
            {
                var developer = await _context.Developers.FindAsync(id);
                if (developer != null)
                {
                    await _developerRepository.DevDeleteAsync(id);
                    TempData["SuccessMessage"] = $"Developer '{developer.Name}' deleted successfully!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Developer not found!";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error deleting developer: {ex.Message}";
            }

            return RedirectToAction("AllDeveloper");
        }

        // ✅ GET: Client Management
        [HttpGet]
        public async Task<IActionResult> ClientManagement()
        {
            try
            {
                var clients = await _projectRepository.GetAllClientsAsync();
                return View(clients);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error loading clients: {ex.Message}";
                return View(new List<Users>());
            }
        }

        // ✅ Helper Method: Load Clients for Dropdown
        private async Task LoadClients()
        {
            try
            {
                var clients = await _projectRepository.GetAllClientsAsync();
                if (clients.Any())
                {
                    ViewBag.Clients = clients.Select(c => new SelectListItem
                    {
                        Value = c.Id,
                        Text = $"{c.FullName} - {c.Email}"
                    }).ToList();
                }
                else
                {
                    ViewBag.Clients = new List<SelectListItem>();
                    ViewBag.NoClientsMessage = "No clients available. Please register clients first.";
                }
            }
            catch (Exception ex)
            {
                ViewBag.Clients = new List<SelectListItem>();
                ViewBag.NoClientsMessage = $"Error loading clients: {ex.Message}";
                Console.WriteLine($"Error loading clients: {ex.Message}");
            }
        }

        // ✅ Helper Method: Get Project Statistics
        private async Task<Dictionary<string, int>> GetProjectStatistics()
        {
            var projects = await _projectRepository.GetAllProjectsAsync();

            return new Dictionary<string, int>
            {
                { "Total", projects.Count() },
                { "NotStarted", projects.Count(p => p.Status.Trim().ToLower() == "not started") },
                { "InProgress", projects.Count(p => p.Status.Trim().ToLower() == "in progress") },
                { "OnHold", projects.Count(p => p.Status.Trim().ToLower() == "on hold") },
                { "Completed", projects.Count(p => p.Status == "Completed") }
            };
        }
    }
}