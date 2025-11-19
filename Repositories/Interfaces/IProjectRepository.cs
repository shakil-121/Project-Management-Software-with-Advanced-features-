using FastPMS.Models.Domain;
using FastPMS.Models.ViewModel;

namespace FastPMS.Repositories.Interfaces
{
    public interface IProjectRepository
    {
        Task<IEnumerable<Project?>> GetAllProjectsAsync();
        Task<Project?> GetProjectById(int id); 
        Task<Project>AddProjectAsync(Project project);
        Task<Project?> UpdateProjectAsync(int id, EditProjectRequest editProjectRequest);
        Task<Project> DeleteProjectAsync(int id);
        Task<byte[]> ExportProjectsToExcelAsync();

        Task AssignClientsToProjectAsync(int projectId, List<string> clientIds, string assignedBy);
        Task<List<Users>> GetAllClientsAsync();
        Task<List<Project>> GetProjectsByClientAsync(string clientId);
        Task<List<Users>> GetAssignedClientsAsync(int projectId);
    }
}
