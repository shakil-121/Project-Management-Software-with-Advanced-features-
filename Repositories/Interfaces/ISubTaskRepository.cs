using FastPMS.Models.Domain;

namespace FastPMS.Repositories.Interfaces
{
    public interface ISubTaskRepository
    {
        Task<SubTask> CreateAsync(SubTask subTask);
        Task<SubTask?> GetByIdAsync(string id);
        Task<List<SubTask>> GetByProjectTaskIdAsync(string projectTaskId);
        Task<List<SubTask>> GetByAssignedToAsync(string userId);
        Task<SubTask> UpdateAsync(SubTask subTask);
        Task<bool> DeleteAsync(string id);
        Task<bool> UpdateStatusAsync(string id, string status, string updatedBy);
        Task<List<SubTask>> GetOverdueSubTasksAsync();
    }
}