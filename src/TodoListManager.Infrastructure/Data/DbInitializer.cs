using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TodoListManager.Infrastructure.Identity;
using TodoListManager.Infrastructure.Persistence;

namespace TodoListManager.Infrastructure.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(TodoDbContext context, UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager)
    {
        await context.Database.MigrateAsync();
        await SeedRolesAsync(roleManager);
        await SeedAdminUserAsync(userManager);
    }

    private static async Task SeedRolesAsync(RoleManager<ApplicationRole> roleManager)
    {
        string[] roleNames = { "Admin", "User" };
        string[] roleDescriptions = { "Administrator role with full access", "Standard user role" };

        for (int i = 0; i < roleNames.Length; i++)
        {
            var roleExist = await roleManager.RoleExistsAsync(roleNames[i]);
            if (!roleExist)
            {
                var role = new ApplicationRole
                {
                    Name = roleNames[i],
                    Description = roleDescriptions[i]
                };
                await roleManager.CreateAsync(role);
            }
        }
    }

    private static async Task SeedAdminUserAsync(UserManager<ApplicationUser> userManager)
    {
        const string adminUsername = "admin";
        const string adminEmail = "admin@todolist.com";
        const string adminPassword = "admin";

        var adminUser = await userManager.FindByNameAsync(adminUsername);
        
        if (adminUser == null)
        {
            adminUser = new ApplicationUser
            {
                UserName = adminUsername,
                Email = adminEmail,
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow
            };

            var result = await userManager.CreateAsync(adminUser, adminPassword);

            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }
    }
}
