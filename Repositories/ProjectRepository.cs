// Repositories/ProjectRepository.cs
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
                worksheet.Cells[1, 1].Value = "Project ID";
                worksheet.Cells[1, 2].Value = "Project Title";
                worksheet.Cells[1, 3].Value = "Project Description";
                worksheet.Cells[1, 4].Value = "Stack";
                worksheet.Cells[1, 5].Value = "Start Date";
                worksheet.Cells[1, 6].Value = "End Date";
                worksheet.Cells[1, 7].Value = "Status";

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
            var existingProject = await pmsDbContext.Projects.FindAsync(id);
            if (existingProject != null)
            {
                existingProject.ProjectTitle = editProjectRequest.ProjectTitle;
                existingProject.ProjectDescription = editProjectRequest.ProjectDescription;
                existingProject.Stack = editProjectRequest.Stack;
                existingProject.StartDate = editProjectRequest.StartDate;
                existingProject.EndDate = editProjectRequest.EndDate;
                existingProject.Status = editProjectRequest.Status;
                await pmsDbContext.SaveChangesAsync();
                return existingProject;
            }
            return null;
        }

        
        public async Task AssignClientsToProjectAsync(int projectId, List<string> clientIds, string assignedBy)
        {
            foreach (var clientId in clientIds)
            {
                var projectUser = new ProjectUser
                {
                    ProjectId = projectId,
                    UserId = clientId,
                    UserRole = "Client",
                    AssignedBy = assignedBy,
                    AssignedDate = DateTime.Now
                };
                await pmsDbContext.ProjectUsers.AddAsync(projectUser);
            }
            await pmsDbContext.SaveChangesAsync();
        }

        public async Task<List<Users>> GetAllClientsAsync()
        {
            return await pmsDbContext.Users
                .Where(u => u.Role == "Client")
                .ToListAsync();
        }

        public async Task<List<Project>> GetProjectsByClientAsync(string clientId)
        {
            return await pmsDbContext.ProjectUsers
                .Where(pu => pu.UserId == clientId && pu.UserRole == "Client")
                .Include(pu => pu.Project)
                .Select(pu => pu.Project)
                .ToListAsync();
        }

        public async Task<List<Users>> GetAssignedClientsAsync(int projectId)
        {
            return await pmsDbContext.ProjectUsers
                .Where(pu => pu.ProjectId == projectId && pu.UserRole == "Client")
                .Include(pu => pu.User)
                .Select(pu => pu.User)
                .ToListAsync();
        }
    }
}