using FastPMS.Models.Domain;
using FastPMS.Models.ViewModel;

namespace FastPMS.Services
{
    public interface ISubTaskService
    {
        Task<SubTask> CreateSubTaskAsync(SubTaskCreateViewModel model, string assignedById);
        Task<SubTask?> GetSubTaskByIdAsync(string id);
        Task<List<SubTask>> GetSubTasksByProjectTaskAsync(string projectTaskId);
        Task<List<SubTask>> GetUserSubTasksAsync(string userId);
        Task<SubTask> UpdateSubTaskAsync(SubTaskEditViewModel model, string updatedById);
        Task<bool> UpdateSubTaskStatusAsync(string id, string status, string updatedBy);
        Task<bool> DeleteSubTaskAsync(string id);
        Task<List<SubTask>> GetOverdueSubTasksAsync(); // ✅ FIXED: Added this method
    }
}