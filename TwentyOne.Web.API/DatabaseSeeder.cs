using Microsoft.AspNetCore.Identity;
using TwentyOne.DAL.Entities;
using TwentyOne.DAL.Repositories.Interfaces;

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

        public static async Task SeedGuestAccountAsync(
            UserManager<ApplicationUser> userManager)
        {
            var guestEmail = "guest@twentyone.com";
            var existingGuest = await userManager.FindByEmailAsync(guestEmail);

            if (existingGuest == null)
            {
                var guest = new ApplicationUser
                {
                    FullName = "Guest Customer",
                    Email = guestEmail,
                    UserName = guestEmail,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                var result = await userManager.CreateAsync(guest, "Guest@12345");
                if (result.Succeeded)
                    await userManager.AddToRoleAsync(guest, "Customer");
            }
        }

        public static async Task SeedSiteSettingsAsync(
            TwentyOne.DAL.Repositories.Interfaces.ISiteSettingRepository
            siteSettingRepository)
        {
            // bKash number
            var bkash = await siteSettingRepository
                .GetValueAsync("BkashNumber");
            if (string.IsNullOrEmpty(bkash))
                await siteSettingRepository
                    .SetValueAsync("BkashNumber", "01749028100");

            // Delivery charges
            var insideDhaka = await siteSettingRepository
                .GetValueAsync("DeliveryCharge_InsideDhaka");
            if (string.IsNullOrEmpty(insideDhaka))
                await siteSettingRepository
                    .SetValueAsync("DeliveryCharge_InsideDhaka", "80");

            var outsideDhaka = await siteSettingRepository
                .GetValueAsync("DeliveryCharge_OutsideDhaka");
            if (string.IsNullOrEmpty(outsideDhaka))
                await siteSettingRepository
                    .SetValueAsync("DeliveryCharge_OutsideDhaka", "130");
        }
    }
}
