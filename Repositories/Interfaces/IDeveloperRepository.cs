using FastPMS.Models.Domain;

namespace FastPMS.Repositories.Interfaces
{
    public interface IDeveloperRepository
    {
        Task<Developer> AddDeveloperAsync(Developer developer);
        Task<IEnumerable<Developer>> GetAllDevelopersAsync();
        Task<Developer> DevDeleteAsync(int id);
    }
}
