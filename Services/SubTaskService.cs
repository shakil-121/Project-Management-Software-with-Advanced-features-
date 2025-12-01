using FastPMS.Data;
using FastPMS.Models.Domain;
using FastPMS.Models.ViewModel;
using FastPMS.Repositories.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FastPMS.Services
{
    public class SubTaskService : ISubTaskService
    {
        private readonly ISubTaskRepository _subTaskRepository;
        private readonly UserManager<Users> _userManager;
        private readonly PmsDbContext _context;
        private readonly ILogger<SubTaskService> _logger;

        public SubTaskService(
            ISubTaskRepository subTaskRepository,
            UserManager<Users> userManager,
            PmsDbContext context,
            ILogger<SubTaskService> logger)
        {
            _subTaskRepository = subTaskRepository;
            _userManager = userManager;
            _context = context;
            _logger = logger;
        }

        public async Task<SubTask> CreateSubTaskAsync(SubTaskCreateViewModel model, string assignedById)
        {
            _logger.LogInformation("=== STARTING SUBTASK CREATION ===");

            try
            {
                _logger.LogInformation($"Model Data - Title: {model.Title}, TaskId: {model.ProjectTaskId}, AssignTo: {model.AssignedToId}");

                // 1. Validate parent task exists
                _logger.LogInformation("Validating parent task...");
                var parentTask = await _context.Tasks
                    .FirstOrDefaultAsync(t => t.Id == model.ProjectTaskId);

                if (parentTask == null)
                {
                    _logger.LogError($"❌ Parent task not found with ID: {model.ProjectTaskId}");
                    throw new ArgumentException($"Parent task not found with ID: {model.ProjectTaskId}");
                }
                _logger.LogInformation($"✅ Parent task found: {parentTask.Title}");

                // 2. Validate assigned user exists
                _logger.LogInformation("Validating assigned user...");
                var assignedUser = await _userManager.FindByIdAsync(model.AssignedToId);
                if (assignedUser == null)
                {
                    _logger.LogError($"❌ Assigned user not found with ID: {model.AssignedToId}");
                    throw new ArgumentException($"Assigned user not found with ID: {model.AssignedToId}");
                }
                _logger.LogInformation($"✅ Assigned user found: {assignedUser.FullName}");

                // 3. Validate assigned by user exists
                _logger.LogInformation("Validating assigned by user...");
                var assignedByUser = await _userManager.FindByIdAsync(assignedById);
                if (assignedByUser == null)
                {
                    _logger.LogError($"❌ Assigned by user not found with ID: {assignedById}");
                    throw new ArgumentException($"Assigned by user not found with ID: {assignedById}");
                }
                _logger.LogInformation($"✅ Assigned by user found: {assignedByUser.FullName}");

                // 4. Create sub-task entity
                _logger.LogInformation("Creating SubTask entity...");
                var subTask = new SubTask
                {
                    Id = Guid.NewGuid().ToString(),
                    Title = model.Title?.Trim() ?? string.Empty,
                    Description = model.Description?.Trim(),
                    ProjectTaskId = model.ProjectTaskId,
                    AssignedToId = model.AssignedToId,
                    AssignedById = assignedById,
                    DueDate = model.DueDate,
                    Priority = model.Priority ?? "Medium",
                    EstimatedHours = model.EstimatedHours,
                    ActualHours = 0, // Default value
                    Notes = model.Notes?.Trim(),
                    Status = "Not Started",
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    CompletedDate = null
                };

                _logger.LogInformation($"✅ SubTask entity created: {subTask.Id}");

                // 5. Save to database
                _logger.LogInformation("Saving to database...");
                var result = await _subTaskRepository.CreateAsync(subTask);

                _logger.LogInformation($"🎉 SUBTASK CREATED SUCCESSFULLY: {result.Id}");
                _logger.LogInformation("=== SUBTASK CREATION COMPLETED ===");

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR in CreateSubTaskAsync");
                throw new Exception($"Failed to create sub-task: {ex.Message}", ex);
            }
        }

        public async Task<SubTask?> GetSubTaskByIdAsync(string id)
        {
            return await _subTaskRepository.GetByIdAsync(id);
        }

        public async Task<List<SubTask>> GetSubTasksByProjectTaskAsync(string projectTaskId)
        {
            return await _subTaskRepository.GetByProjectTaskIdAsync(projectTaskId);
        }

        public async Task<List<SubTask>> GetUserSubTasksAsync(string userId)
        {
            return await _subTaskRepository.GetByAssignedToAsync(userId);
        }

        public async Task<SubTask> UpdateSubTaskAsync(SubTaskEditViewModel model, string updatedById)
        {
            var existingSubTask = await _subTaskRepository.GetByIdAsync(model.Id);
            if (existingSubTask == null)
                throw new ArgumentException("Sub-task not found");

            // Validate parent task exists
            var parentTask = await _context.Tasks.FindAsync(model.ProjectTaskId);
            if (parentTask == null)
                throw new ArgumentException("Parent task not found");

            // Validate assigned user exists
            var assignedUser = await _userManager.FindByIdAsync(model.AssignedToId);
            if (assignedUser == null)
                throw new ArgumentException("Assigned user not found");

            existingSubTask.Title = model.Title.Trim();
            existingSubTask.Description = model.Description?.Trim();
            existingSubTask.ProjectTaskId = model.ProjectTaskId;
            existingSubTask.AssignedToId = model.AssignedToId;
            existingSubTask.DueDate = model.DueDate;
            existingSubTask.Priority = model.Priority;
            existingSubTask.EstimatedHours = model.EstimatedHours;
            existingSubTask.ActualHours = model.ActualHours;
            existingSubTask.Notes = model.Notes?.Trim();
            existingSubTask.Status = model.Status;
            existingSubTask.UpdatedAt = DateTime.Now;

            // Set completion date if status changed to Completed
            if (model.Status == "Completed" && existingSubTask.CompletedDate == null)
            {
                existingSubTask.CompletedDate = DateTime.Now;
            }
            else if (model.Status != "Completed")
            {
                existingSubTask.CompletedDate = null;
            }

            return await _subTaskRepository.UpdateAsync(existingSubTask);
        }

        public async Task<bool> UpdateSubTaskStatusAsync(string id, string status, string updatedBy)
        {
            return await _subTaskRepository.UpdateStatusAsync(id, status, updatedBy);
        }

        public async Task<bool> DeleteSubTaskAsync(string id)
        {
            return await _subTaskRepository.DeleteAsync(id);
        }

        public async Task<List<SubTask>> GetOverdueSubTasksAsync()
        {
            return await _subTaskRepository.GetOverdueSubTasksAsync();
        }
    }
}