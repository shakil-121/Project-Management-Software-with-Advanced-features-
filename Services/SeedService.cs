using FastPMS.Data;
using FastPMS.Models.Domain;
using Microsoft.AspNetCore.Identity;

namespace FastPMS.Services
{
    public class SeedService
    { 
        public static async Task SeedDatabase(IServiceProvider serviceProvider)
        {
            using var scope=serviceProvider.CreateScope(); 
            var context=scope.ServiceProvider.GetRequiredService<PmsDbContext>(); 
            var roleManager=scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<Users>>(); 
            var logger=scope.ServiceProvider.GetRequiredService <ILogger<SeedService>>();


            try
            {
                logger.LogInformation("Ensuring the database is created.");
                await context.Database.EnsureCreatedAsync();

                // All required roles
                string[] roles = { "SuperAdmin", "Admin", "TeamMember", "Client" };

                logger.LogInformation("Seeding roles...");
                foreach (var role in roles)
                {
                    await AddRoleAsync(roleManager, role);
                }

                // SuperAdmin user seed
                logger.LogInformation("Seeding SuperAdmin user.");

                var superAdminEmail = "superadmin@fastpms.com";

                if (await userManager.FindByEmailAsync(superAdminEmail) == null)
                {
                    var superAdmin = new Users
                    {
                        FullName = "System SuperAdmin",
                        UserName = superAdminEmail,
                        Email = superAdminEmail,
                        NormalizedEmail = superAdminEmail.ToUpper(),
                        NormalizedUserName = superAdminEmail.ToUpper(),
                        EmailConfirmed = true
                    };

                    var result = await userManager.CreateAsync(superAdmin, "SuperAdmin@123");

                    if (result.Succeeded)
                    {
                        logger.LogInformation("Assigning SuperAdmin role to SuperAdmin user.");
                        await userManager.AddToRoleAsync(superAdmin, "SuperAdmin");
                    }
                    else
                    {
                        logger.LogError("Failed to create SuperAdmin user: {Errors}",
                            string.Join(", ", result.Errors.Select(e => e.Description)));
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while seeding the database.");
            }
        }
        private static async Task AddRoleAsync(RoleManager<IdentityRole> roleManager, string roleName)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                var result = await roleManager.CreateAsync(new IdentityRole(roleName));
                if (!result.Succeeded)
                {
                    throw new Exception(
                        $"Failed to create role '{roleName}': {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }
        }
    }
}
