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

    }
}
