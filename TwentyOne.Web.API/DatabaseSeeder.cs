using Microsoft.AspNetCore.Identity;
using TwentyOne.DAL.Entities;

namespace TwentyOne.Web.API
{
    public static class DatabaseSeeder
    {
        public static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
        {
            string[] roles = { "SuperAdmin", "Admin", "Customer" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    //continue;
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }
        }

        public static async Task SeedSuperAdminAsync(
            UserManager<ApplicationUser> userManager)
        {
            var superAdminEmail = "superadmin@twentyone.com";

            var existingUser = await userManager
                .FindByEmailAsync(superAdminEmail);

            if (existingUser == null)
            {
                var superAdmin = new ApplicationUser
                {
                    FullName = "Super Admin",
                    Email = superAdminEmail,
                    UserName = superAdminEmail,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                var result = await userManager
                    .CreateAsync(superAdmin, "Admin@12345");

                if (result.Succeeded)
                {
                    await userManager
                        .AddToRoleAsync(superAdmin, "SuperAdmin");
                }
            }
        }
    }
}
