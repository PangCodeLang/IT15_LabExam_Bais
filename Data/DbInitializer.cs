using IT15_LabExam_Bais.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace IT15_LabExam_Bais.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(IServiceProvider services, IConfiguration configuration)
    {
        using var scope = services.CreateScope();
        var scopedServices = scope.ServiceProvider;

        var context = scopedServices.GetRequiredService<ApplicationDbContext>();
        await context.Database.MigrateAsync();

        var roleManager = scopedServices.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scopedServices.GetRequiredService<UserManager<IdentityUser>>();

        await EnsureRoleAsync(roleManager, "Admin");
        await EnsureRoleAsync(roleManager, "Staff");

        var adminEmail = configuration["SeedAccounts:AdminEmail"] ?? "admin@it15.local";
        var adminPassword = configuration["SeedAccounts:AdminPassword"] ?? "Admin#123";
        await EnsureUserAsync(userManager, adminEmail, adminPassword, "Admin");

        var staffEmail = configuration["SeedAccounts:StaffEmail"] ?? "staff@it15.local";
        var staffPassword = configuration["SeedAccounts:StaffPassword"] ?? "Staff#123";
        await EnsureUserAsync(userManager, staffEmail, staffPassword, "Staff");

        if (!await context.Students.AnyAsync())
        {
            context.Students.AddRange(
                new Student
                {
                    StudentNumber = "2026-0001",
                    FirstName = "Andrea",
                    LastName = "Cruz",
                    Email = "andrea.cruz@example.com",
                    Course = "BS Information Technology",
                    YearLevel = 2,
                    BirthDate = new DateOnly(2005, 3, 12)
                },
                new Student
                {
                    StudentNumber = "2026-0002",
                    FirstName = "Marco",
                    LastName = "Santos",
                    Email = "marco.santos@example.com",
                    Course = "BS Computer Science",
                    YearLevel = 3,
                    BirthDate = new DateOnly(2004, 10, 4)
                },
                new Student
                {
                    StudentNumber = "2026-0003",
                    FirstName = "Leah",
                    LastName = "Garcia",
                    Email = "leah.garcia@example.com",
                    Course = "BS Information Systems",
                    YearLevel = 1,
                    BirthDate = new DateOnly(2006, 7, 28)
                }
            );

            await context.SaveChangesAsync();
        }
    }

    private static async Task EnsureRoleAsync(RoleManager<IdentityRole> roleManager, string roleName)
    {
        if (!await roleManager.RoleExistsAsync(roleName))
        {
            await roleManager.CreateAsync(new IdentityRole(roleName));
        }
    }

    private static async Task EnsureUserAsync(
        UserManager<IdentityUser> userManager,
        string email,
        string password,
        string role)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
        {
            user = new IdentityUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true
            };

            var createResult = await userManager.CreateAsync(user, password);
            if (!createResult.Succeeded)
            {
                return;
            }
        }

        if (!await userManager.IsInRoleAsync(user, role))
        {
            await userManager.AddToRoleAsync(user, role);
        }
    }
}
