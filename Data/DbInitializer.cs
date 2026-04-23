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

        if (!await context.Employees.AnyAsync())
        {
            var employees = new List<Employee>
            {
                new()
                {
                    FirstName = "Ana",
                    LastName = "Reyes",
                    Position = "HR Officer",
                    Department = "Human Resources",
                    DailyRate = 1500m
                },
                new()
                {
                    FirstName = "Mark",
                    LastName = "Dela Cruz",
                    Position = "Software Developer",
                    Department = "IT",
                    DailyRate = 2200m
                }
            };

            context.Employees.AddRange(employees);
            await context.SaveChangesAsync();

            var payrollRecords = new List<PayrollRecord>
            {
                new()
                {
                    EmployeeId = employees[0].Id,
                    Date = DateTime.Today,
                    DaysWorked = 10,
                    Deduction = 500,
                    GrossPay = 10 * employees[0].DailyRate,
                    NetPay = (10 * employees[0].DailyRate) - 500
                },
                new()
                {
                    EmployeeId = employees[1].Id,
                    Date = DateTime.Today,
                    DaysWorked = 8,
                    Deduction = 800,
                    GrossPay = 8 * employees[1].DailyRate,
                    NetPay = (8 * employees[1].DailyRate) - 800
                }
            };

            context.PayrollRecords.AddRange(payrollRecords);
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
