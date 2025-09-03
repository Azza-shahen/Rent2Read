using Microsoft.AspNetCore.Identity;

namespace Rent2Read.Web.Seeds
{
    public static class DefaultUsers
    {
        public static async Task SeedAdminUserAsync(UserManager<ApplicationUser> userManager)
        {
            ApplicationUser admin = new()
            {
                UserName = "admin",
                Email = "admin@Rent2Read.Com",
                EmailConfirmed = true,
                FullName = "admin"
            };
            var user = await userManager.FindByEmailAsync(admin.Email);
            if (user == null)
            {
                await userManager.CreateAsync(admin, "P@sswoed123");
                await userManager.AddToRoleAsync(admin, AppRoles.Admin);
            }
        }
    }
}
