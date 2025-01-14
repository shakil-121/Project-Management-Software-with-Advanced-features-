using FastPMS.Models.Domain;
using Microsoft.EntityFrameworkCore;

namespace FastPMS.Data
{
    public class PmsDbContext:DbContext
    {
        public PmsDbContext(DbContextOptions options):base(options)
        {
            
        } 
        public DbSet<Project> Projects { get; set; }  
        public DbSet<Developer> Developers { get; set; }
    }
}
