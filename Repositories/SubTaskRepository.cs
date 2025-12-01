using Microsoft.EntityFrameworkCore;
using FastPMS.Data;
using FastPMS.Models.Domain;
using FastPMS.Repositories.Interfaces;

namespace FastPMS.Repositories
{
    public class SubTaskRepository : ISubTaskRepository
    {
        private readonly PmsDbContext _context;

        public SubTaskRepository(PmsDbContext context)
        {
            _context = context;
        }

        public async Task<SubTask> CreateAsync(SubTask subTask)
        {
            subTask.CreatedAt = DateTime.Now;
            subTask.UpdatedAt = DateTime.Now;

            _context.SubTasks.Add(subTask);
            await _context.SaveChangesAsync();
            return subTask;
        }

        public async Task<SubTask?> GetByIdAsync(string id)
        {
            return await _context.SubTasks
                .Include(st => st.ProjectTask)
                    .ThenInclude(pt => pt!.Project)
                .Include(st => st.AssignedTo)
                .Include(st => st.AssignedBy)
                .FirstOrDefaultAsync(st => st.Id == id);
        }

        public async Task<List<SubTask>> GetByProjectTaskIdAsync(string projectTaskId)
        {
            return await _context.SubTasks
                .Include(st => st.AssignedTo)
                .Include(st => st.ProjectTask)
                .Where(st => st.ProjectTaskId == projectTaskId)
                .OrderByDescending(st => st.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<SubTask>> GetByAssignedToAsync(string userId)
        {
            return await _context.SubTasks
                .Include(st => st.ProjectTask)
                    .ThenInclude(pt => pt!.Project)
                .Where(st => st.AssignedToId == userId)
                .OrderByDescending(st => st.DueDate)
                .ToListAsync();
        }

        public async Task<SubTask> UpdateAsync(SubTask subTask)
        {
            subTask.UpdatedAt = DateTime.Now;
            _context.SubTasks.Update(subTask);
            await _context.SaveChangesAsync();
            return subTask;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var subTask = await _context.SubTasks.FindAsync(id);
            if (subTask == null) return false;

            _context.SubTasks.Remove(subTask);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateStatusAsync(string id, string status, string updatedBy)
        {
            var subTask = await _context.SubTasks.FindAsync(id);
            if (subTask == null) return false;

            subTask.Status = status;
            subTask.UpdatedAt = DateTime.Now;

            if (status == "Completed")
            {
                subTask.CompletedDate = DateTime.Now;
            }
            else
            {
                subTask.CompletedDate = null;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<SubTask>> GetOverdueSubTasksAsync()
        {
            return await _context.SubTasks
                .Include(st => st.AssignedTo)
                .Include(st => st.ProjectTask)
                .Where(st => st.DueDate < DateTime.Now && st.Status != "Completed")
                .ToListAsync();
        }
    }
}