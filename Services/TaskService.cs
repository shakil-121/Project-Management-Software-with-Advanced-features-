using Microsoft.EntityFrameworkCore;
using FastPMS.Data;
using FastPMS.Models.Domain;
using FastPMS.Models.ViewModel;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FastPMS.Services
{
    public class TaskService : ITaskService
    {
        private readonly PmsDbContext _context;
        private readonly INotificationService _notificationService;

        public TaskService(PmsDbContext context, INotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        public async Task<ProjectTask> CreateTaskAsync(CreateTaskRequest request, string assignedById)
        {
            var task = new ProjectTask
            {
                Title = request.Title,
                Description = request.Description,
                ProjectId = request.ProjectId,
                AssignedToId = request.AssignedToId,
                AssignedById = assignedById,
                Priority = request.Priority,
                DueDate = request.DueDate,
                EstimatedHours = request.EstimatedHours,
                Status = "Not Started",
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();

            // ✅ Safe notification creation
            try
            {
                await _notificationService.CreateNotificationAsync(
                    request.AssignedToId,
                    $"New task assigned: {request.Title} (Due: {request.DueDate:MMM dd, yyyy})",
                    "task",
                    task.Id
                );
            }
            catch (Exception ex)
            {
                // Log notification error but don't fail the task creation
                Console.WriteLine($"Notification error: {ex.Message}");
            }

            return task;
        }

        public async Task<List<ProjectTask>> GetTasksByProjectAsync(int projectId)
        {
            return await _context.Tasks
                .Include(t => t.AssignedTo)
                .Include(t => t.Project)
                .Where(t => t.ProjectId == projectId)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<ProjectTask>> GetTasksByUserAsync(string userId)
        {
            return await _context.Tasks
                .Include(t => t.AssignedTo)
                .Include(t => t.Project)
                .Where(t => t.AssignedToId == userId || t.AssignedById == userId)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<ProjectTask>> GetTasksByAssignedUserAsync(string assignedToId)
        {
            return await _context.Tasks
                .Include(t => t.Project)
                .Include(t => t.AssignedBy)
                .Where(t => t.AssignedToId == assignedToId)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task<ProjectTask?> GetTaskByIdAsync(string taskId)
        {
            return await _context.Tasks
                .Include(t => t.AssignedTo)
                .Include(t => t.Project)
                .Include(t => t.AssignedBy)
                .FirstOrDefaultAsync(t => t.Id == taskId);
        }

        public async Task<ProjectTask?> UpdateTaskAsync(string taskId, CreateTaskRequest request)
        {
            var task = await _context.Tasks.FindAsync(taskId);
            if (task == null) return null;

            task.Title = request.Title;
            task.Description = request.Description;
            task.ProjectId = request.ProjectId;
            task.AssignedToId = request.AssignedToId;
            task.Priority = request.Priority;
            task.DueDate = request.DueDate;
            task.EstimatedHours = request.EstimatedHours;
            task.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            return task;
        }

        // ✅ FIXED: UpdateTaskStatusAsync with better error handling
        public async Task<bool> UpdateTaskStatusAsync(string taskId, string status)
        {
            try
            {
                Console.WriteLine($"UpdateTaskStatusAsync called - TaskId: {taskId}, Status: {status}");

                // Validate inputs
                if (string.IsNullOrEmpty(taskId) || string.IsNullOrEmpty(status))
                {
                    Console.WriteLine("Invalid taskId or status");
                    return false;
                }

                // Get task with tracking
                var task = await _context.Tasks
                    .FirstOrDefaultAsync(t => t.Id == taskId);

                if (task == null)
                {
                    Console.WriteLine($"Task not found with ID: {taskId}");
                    return false;
                }

                Console.WriteLine($"Task found: {task.Title}, Current status: {task.Status}");

                // Validate status value
                var validStatuses = new[] { "Not Started", "In Progress", "Completed", "On Hold" };
                if (!validStatuses.Contains(status))
                {
                    Console.WriteLine($"Invalid status value: {status}");
                    return false;
                }

                // Update task properties
                task.Status = status;
                task.UpdatedAt = DateTime.Now;

                // Handle completed status
                if (status == "Completed")
                {
                    task.CompletedDate = DateTime.Now;
                    Console.WriteLine("Task marked as completed");
                }
                else
                {
                    task.CompletedDate = null;
                }

                // Save changes
                var result = await _context.SaveChangesAsync();
                Console.WriteLine($"SaveChanges result: {result} rows affected");

                // Send notification if status changed to completed
                if (status == "Completed")
                {
                    try
                    {
                        await _notificationService.CreateNotificationAsync(
                            task.AssignedById, // Notify the assigner
                            $"Task completed: {task.Title}",
                            "task_completed",
                            task.Id
                        );
                    }
                    catch (Exception notifEx)
                    {
                        Console.WriteLine($"Notification error (non-critical): {notifEx.Message}");
                        // Don't fail the status update if notification fails
                    }
                }

                return result > 0;
            }
            catch (DbUpdateException dbEx)
            {
                Console.WriteLine($"Database update error: {dbEx.Message}");
                Console.WriteLine($"Inner exception: {dbEx.InnerException?.Message}");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"General error in UpdateTaskStatusAsync: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return false;
            }
        }

        // ✅ ALTERNATIVE: Simple version without notifications
        public async Task<bool> UpdateTaskStatusSimpleAsync(string taskId, string status)
        {
            try
            {
                var task = await _context.Tasks.FindAsync(taskId);
                if (task == null) return false;

                task.Status = status;
                task.UpdatedAt = DateTime.Now;

                if (status == "Completed")
                    task.CompletedDate = DateTime.Now;

                var result = await _context.SaveChangesAsync();
                return result > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Simple update error: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteTaskAsync(string taskId)
        {
            try
            {
                var task = await _context.Tasks.FindAsync(taskId);
                if (task == null) return false;

                _context.Tasks.Remove(task);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Delete task error: {ex.Message}");
                return false;
            }
        }

        public async Task<List<ProjectTask>> GetOverdueTasksAsync()
        {
            return await _context.Tasks
                .Include(t => t.AssignedTo)
                .Include(t => t.Project)
                .Where(t => t.DueDate < DateTime.Now && t.Status != "Completed")
                .OrderBy(t => t.DueDate)
                .ToListAsync();
        }

        public async Task<int> GetTaskCountByStatusAsync(string status)
        {
            return await _context.Tasks
                .CountAsync(t => t.Status == status);
        }  


    }
}