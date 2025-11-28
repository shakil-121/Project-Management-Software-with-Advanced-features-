using FastPMS.Models.Domain;
using FastPMS.Models.ViewModel;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FastPMS.Services
{
    public interface ITaskService
    {
        Task<ProjectTask> CreateTaskAsync(CreateTaskRequest request, string assignedById);
        Task<List<ProjectTask>> GetTasksByProjectAsync(int projectId); // ✅ Change to int
        Task<List<ProjectTask>> GetTasksByUserAsync(string userId);
        Task<List<ProjectTask>> GetTasksByAssignedUserAsync(string assignedToId);
        Task<ProjectTask?> GetTaskByIdAsync(string taskId);
        Task<ProjectTask?> UpdateTaskAsync(string taskId, CreateTaskRequest request);
        Task<bool> UpdateTaskStatusAsync(string taskId, string status);
        Task<bool> DeleteTaskAsync(string taskId);
        Task<List<ProjectTask>> GetOverdueTasksAsync();
        Task<int> GetTaskCountByStatusAsync(string status);
        Task<bool> UpdateTaskStatusSimpleAsync(string taskId, string status);
    }
}