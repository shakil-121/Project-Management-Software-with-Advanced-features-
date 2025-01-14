using FastPMS.Data;
using FastPMS.Models.Domain;
using FastPMS.Models.ViewModel;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using System.IO;
using System.Linq;


namespace FastPMS.Controllers
{
    public class AdminProjectController : Controller
    {
        private readonly PmsDbContext PMSDbContext;

        public AdminProjectController(PmsDbContext pmsDbContext)
        {
            this.PMSDbContext = pmsDbContext;
        }

        [HttpGet]
        public IActionResult CreateProject()
        {
            return View();
        }

        [HttpPost]
        public IActionResult CreateProject(CreateProjectRequest createProjectRequest)
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

            PMSDbContext.Projects.Add(project);
            // Store success message in TempData
            TempData["SuccessMessage"] = "Project Create successfully!";
            PMSDbContext.SaveChanges();
            return RedirectToAction("AllProject", "AdminProject");
        }

        [HttpGet]
        public IActionResult AllProject()
        {
            var projects = PMSDbContext.Projects.ToList();

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
        public IActionResult ExportToExcel()
        {
            var projects = PMSDbContext.Projects.ToList(); // Fetch all projects from the database

            // Create a new Excel package
            using (var package = new ExcelPackage())
            {
                // Add a new worksheet to the workbook
                var worksheet = package.Workbook.Worksheets.Add("Projects");

                // Set the column headers
                worksheet.Cells[1, 1].Value = "Project ID";
                worksheet.Cells[1, 2].Value = "Project Title";
                worksheet.Cells[1, 3].Value = "Project Description";
                worksheet.Cells[1, 4].Value = "Stack";
                worksheet.Cells[1, 5].Value = "Start Date";
                worksheet.Cells[1, 6].Value = "End Date";
                worksheet.Cells[1, 7].Value = "Status";

                // Populate rows with project data
                for (int i = 0; i < projects.Count; i++)
                {
                    var project = projects[i];
                    worksheet.Cells[i + 2, 1].Value = project.ProjectId;
                    worksheet.Cells[i + 2, 2].Value = project.ProjectTitle;
                    worksheet.Cells[i + 2, 3].Value = project.ProjectDescription;
                    worksheet.Cells[i + 2, 4].Value = project.Stack;
                    worksheet.Cells[i + 2, 5].Value = project.StartDate.ToString("yyyy-MM-dd");
                    worksheet.Cells[i + 2, 6].Value = project.EndDate.ToString("yyyy-MM-dd");
                    worksheet.Cells[i + 2, 7].Value = project.Status;
                }

                // Auto-fit columns for readability
                worksheet.Cells.AutoFitColumns();

                // Convert the Excel package to a byte array
                var excelData = package.GetAsByteArray();

                // Return the file for download
                return File(excelData, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Projects.xlsx");
            }


        }
        [HttpGet]
        public IActionResult EditProject(int id)
        {
            var project = PMSDbContext.Projects.FirstOrDefault(x => x.ProjectId == id);
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
        public IActionResult EditProject(EditProjectRequest editProjectRequest)
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

            // Fetch the existing project from the database using ProjectId
            var existingProject = PMSDbContext.Projects.Find(editProjectRequest.ProjectId);
            if (existingProject != null)
            {
                // Update the existing project's properties
                existingProject.ProjectTitle = editProjectRequest.ProjectTitle;
                existingProject.ProjectDescription = editProjectRequest.ProjectDescription;
                existingProject.Stack = editProjectRequest.Stack;
                existingProject.StartDate = editProjectRequest.StartDate;
                existingProject.EndDate = editProjectRequest.EndDate;
                existingProject.Status = editProjectRequest.Status;

                // Save changes to the database
                PMSDbContext.SaveChanges();

                // Store success message in TempData
                TempData["updateSuccess"] = "Project updated successfully!";
            }
            return RedirectToAction("AllProject");
        }
        [HttpPost]
        public IActionResult DeleteProject(int id)
        {
            var project = PMSDbContext.Projects.Find(id);
            if (project != null)
            {
                PMSDbContext.Projects.Remove(project);
                PMSDbContext.SaveChanges(); // Don't forget to save changes after removal
            }

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

               await PMSDbContext.AddAsync(developer);
                await PMSDbContext.SaveChangesAsync();
                return RedirectToAction("AllProject", "AdminProject");
            }
            return View(developer);



        }
    }
}
