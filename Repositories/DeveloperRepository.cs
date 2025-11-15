using FastPMS.Data;
using FastPMS.Models.Domain;
using FastPMS.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FastPMS.Repositories
{
    public class DeveloperRepository : IDeveloperRepository
    {
        private readonly PmsDbContext _pmsDbContext;

        public DeveloperRepository(PmsDbContext pmsDbContext)
        {
            _pmsDbContext = pmsDbContext;
        }

        // Add developer to the database
        public async Task<Developer> AddDeveloperAsync(Developer developer)
        {
            await _pmsDbContext.Developers.AddAsync(developer);
            await _pmsDbContext.SaveChangesAsync();
            return developer;
        }

        // Retrieve all developers from the database
        public async Task<IEnumerable<Developer>> GetAllDevelopersAsync()
        {
            return await _pmsDbContext.Developers.ToListAsync();
        }
       
        public async Task<Developer> DevDeleteAsync(int id)
        {
            var dev = await _pmsDbContext.Developers.FindAsync(id);
            if (dev != null)
            {
                _pmsDbContext.Developers.Remove(dev);
                await _pmsDbContext.SaveChangesAsync();
            }
            return dev;
        }
    }
}
