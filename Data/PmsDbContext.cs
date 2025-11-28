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

        public DbSet<ProjectUser> ProjectUsers { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<ProjectTask> Tasks { get; set; }



        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<IdentityUserRole<string>>(entity => entity.Ignore("UsersId"));
            builder.Entity<ProjectTask>(entity => {
                entity.Property(t => t.EstimatedHours).HasPrecision(10, 2);
                entity.Property(t => t.ActualHours).HasPrecision(10, 2);
            });



            // ✅ ONLY configure NEW tables, don't modify existing ones
            builder.Entity<ProjectTask>(entity =>
            {
                entity.HasKey(t => t.Id);
                entity.Property(t => t.Title).HasMaxLength(200);
                entity.Property(t => t.Description).HasMaxLength(1000);
                entity.Property(t => t.Status).HasMaxLength(50);
                entity.Property(t => t.Priority).HasMaxLength(50);

                // ✅ Decimal precision
                entity.Property(t => t.EstimatedHours).HasPrecision(10, 2);
                entity.Property(t => t.ActualHours).HasPrecision(10, 2);

                // Relationships
                entity.HasOne(t => t.Project)
                      .WithMany()
                      .HasForeignKey(t => t.ProjectId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(t => t.AssignedTo)
                      .WithMany()
                      .HasForeignKey(t => t.AssignedToId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(t => t.AssignedBy)
                      .WithMany()
                      .HasForeignKey(t => t.AssignedById)
                      .OnDelete(DeleteBehavior.Restrict);
            });


            // ✅ NOTIFICATION CONFIGURATION
            builder.Entity<Notification>(entity =>
            {
                entity.HasKey(n => n.Id);

                entity.HasOne(n => n.User)
                    .WithMany(u => u.Notifications)
                    .HasForeignKey(n => n.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.Property(n => n.Message)
                    .IsRequired()
                    .HasMaxLength(500);

                entity.Property(n => n.Type)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(n => n.RelatedId)
                    .HasMaxLength(100);

                entity.HasIndex(n => n.UserId);
                entity.HasIndex(n => new { n.UserId, n.IsRead });
                entity.HasIndex(n => n.CreatedAt);
            });


            builder.Entity<ProjectUser>(entity =>
            {
                entity.HasKey(pu => pu.Id);

                entity.HasOne(pu => pu.Project)
                    .WithMany(p => p.ProjectUsers)
                    .HasForeignKey(pu => pu.ProjectId);

                entity.HasOne(pu => pu.User)
                    .WithMany(u => u.ProjectUsers)
                    .HasForeignKey(pu => pu.UserId);
            });


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