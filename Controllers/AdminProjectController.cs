using FastPMS.Data;
using FastPMS.Models.Domain;
using FastPMS.Models.ViewModel;
using FastPMS.Repositories;
using FastPMS.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using System.IO;
using System.Linq;


namespace FastPMS.Controllers
{
    public class AdminProjectController : Controller
    {
        private readonly ProjectRepository projectRepository;
        private readonly DeveloperRepository developerRepository;

        // Combine both dependencies into a single constructor
        public AdminProjectController(IProjectRepository projectRepository, IDeveloperRepository developerRepository)
        {
            this.projectRepository = (ProjectRepository?)projectRepository;
            this.developerRepository = (DeveloperRepository?)developerRepository;
        }

        [HttpGet]
        public IActionResult CreateProject()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateProject(CreateProjectRequest createProjectRequest)
        {
            var project = new Project
            {
                ProjectId = createProjectRequest.ProjectId,
                ProjectTitle = createProjectRequest.ProjectTitle,
                ProjectDescription = createProjectRequest.ProjectDescription,
                Stack = createProjectRequest.Stack,
                StartDate = createProjectRequest.StartDate,
                EndDate = createProjectRequest.EndDate,
                Status = createProjectRequest.Status,

            };

            await projectRepository.AddProjectAsync(project);
            // Store success message in TempData
            TempData["SuccessMessage"] = "Project Create successfully!";
            return RedirectToAction("AllProject", "AdminProject");
        }

        [HttpGet]
        public async Task<IActionResult> AllProject()
        {
            var projects=await projectRepository.GetAllProjectsAsync();

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

            return View(projects);
        }


        // Action to export all projects to Excel
        [HttpGet]
        public async Task<IActionResult> ExportToExcel()
        {
            var excelData = await projectRepository.ExportProjectsToExcelAsync();
            return File(excelData, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Projects.xlsx");

        }
        [HttpGet]
        public async Task<IActionResult> EditProject(int id)
        {
            var project =await projectRepository.GetProjectById(id);
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
            return View(null);
        }

        [HttpPost]
        public async Task<IActionResult> EditProject(EditProjectRequest editProjectRequest)
        {
            var project = new Project
            {
                ProjectId = editProjectRequest.ProjectId,
                ProjectTitle = editProjectRequest.ProjectTitle,
                ProjectDescription = editProjectRequest.ProjectDescription,
                Stack = editProjectRequest.Stack,
                StartDate = editProjectRequest.StartDate,
                EndDate = editProjectRequest.EndDate,
                Status = editProjectRequest.Status,
            };


            var updatedProject = await projectRepository.UpdateProjectAsync(editProjectRequest.ProjectId, editProjectRequest);

            if (updatedProject != null)
            {
                TempData["updateSuccess"] = "Project updated successfully!";
            }
            else
            {
                TempData["updateFail"] = "Project not found!";
            }

            return RedirectToAction("AllProject");
        } 


        [HttpPost]
        public async Task<IActionResult> DeleteProject(int id)
        {
            var project = await projectRepository.DeleteProjectAsync(id);

            // Redirect back to All Projects page or wherever you want
            return RedirectToAction("AllProject");
        }

        [HttpGet]
        public IActionResult AddDev()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddDev(Developer developer )
        {
            if (ModelState.IsValid)
            {
                if (developer.ImageFile != null)
                {
                    using (var ms = new MemoryStream())
                    {
                        IFormFile? imageFile = developer.ImageFile;
                        await imageFile.CopyToAsync(ms);
                        developer.image = ms.ToArray();
                    }
                }

                await developerRepository.AddDeveloperAsync(developer);
                return RedirectToAction("AllDeveloper", "AdminProject");
            }

            return View(developer);



        }

        //All Developer rettrive from database 
        [HttpGet] 
        public async Task<IActionResult> AllDeveloper()
        {
            var developers = await developerRepository.GetAllDevelopersAsync();
            return View(developers);
        }

        [HttpPost] 
        public async Task<IActionResult> DevDelete(int id)
        { 
            var dev=await developerRepository.DevDeleteAsync(id);
            return RedirectToAction("AllDeveloper");
        }
    }
}
