using FastPMS.Data;
using FastPMS.Models.Domain;
using FastPMS.Models.ViewModel;
using FastPMS.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;

namespace FastPMS.Repositories
{
    public class ProjectRepository : IProjectRepository
    {
        private readonly PmsDbContext pmsDbContext;

        public ProjectRepository(PmsDbContext pmsDbContext)
        {
            this.pmsDbContext = pmsDbContext;
        }
        public async Task<Project> AddProjectAsync(Project project)
        {
            await pmsDbContext.Projects.AddAsync(project);
            await pmsDbContext.SaveChangesAsync(); 
            return project;
        }

        public async Task<Project> DeleteProjectAsync(int id)
        {
            var project = await pmsDbContext.Projects.FindAsync(id);
            if (project != null)
            {
                pmsDbContext.Projects.Remove(project);
                await pmsDbContext.SaveChangesAsync(); 
            }

            return project;
        }

        public async Task<byte[]> ExportProjectsToExcelAsync()
        {
            var projects = await pmsDbContext.Projects.ToListAsync(); 

            using (var package = new ExcelPackage())
            {
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

                worksheet.Cells.AutoFitColumns();

                // Convert the Excel package to a byte array
                return package.GetAsByteArray();
            }
        }

        public async Task<IEnumerable<Project?>> GetAllProjectsAsync()
        {
            var projects = await pmsDbContext.Projects.ToListAsync();

            return projects;
        }

        public async Task<Project?> GetProjectById(int id)
        {
            var project = await pmsDbContext.Projects.FirstOrDefaultAsync(x => x.ProjectId == id); 
            return project;
        }

        public async Task<Project?> UpdateProjectAsync(int id, EditProjectRequest editProjectRequest)
        {
            // Fetch the existing project from the database using the provided id
            var existingProject = await pmsDbContext.Projects.FindAsync(id);

            // If the project is found, update its properties
            if (existingProject != null)
            {
                existingProject.ProjectTitle = editProjectRequest.ProjectTitle;
                existingProject.ProjectDescription = editProjectRequest.ProjectDescription;
                existingProject.Stack = editProjectRequest.Stack;
                existingProject.StartDate = editProjectRequest.StartDate;
                existingProject.EndDate = editProjectRequest.EndDate;
                existingProject.Status = editProjectRequest.Status;

                // Save the changes to the database
                await pmsDbContext.SaveChangesAsync();

                return existingProject; // Return the updated project
            }

            return null; // Return null if the project doesn't exist
        }
    }
}
