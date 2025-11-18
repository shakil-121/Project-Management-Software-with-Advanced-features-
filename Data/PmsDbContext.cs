using FastPMS.Models;
using FastPMS.Models.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FastPMS.Data
{
    public class PmsDbContext : IdentityDbContext<Users>
    {
        public PmsDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<Project> Projects { get; set; }
        public DbSet<Developer> Developers { get; set; }
        public DbSet<LiveChat> LiveChats { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure LiveChats entity to avoid cascade delete issues
            builder.Entity<LiveChat>(entity =>
            {
                entity.HasOne(lc => lc.Sender)
                      .WithMany()
                      .HasForeignKey(lc => lc.SenderId)
                      .OnDelete(DeleteBehavior.Restrict); // Change from Cascade to Restrict

                entity.HasOne(lc => lc.Receiver)
                      .WithMany()
                      .HasForeignKey(lc => lc.ReceiverId)
                      .OnDelete(DeleteBehavior.Restrict); // Change from Cascade to Restrict
            });

            // Remove the unwanted migration that tried to add UsersId to AspNetUserRoles
            builder.Entity<IdentityUserRole<string>>(entity =>
            {
                entity.Ignore("UsersId"); // Ignore this property if it was accidentally added
            });
        }
    }
}