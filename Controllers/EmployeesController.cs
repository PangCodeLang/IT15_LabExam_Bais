using IT15_LabExam_Bais.Data;
using IT15_LabExam_Bais.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IT15_LabExam_Bais.Controllers;

[Authorize(Roles = "Admin,Staff")]
public class EmployeesController(ApplicationDbContext context) : Controller
{
    public async Task<IActionResult> Index()
    {
        var employees = await context.Employees
            .AsNoTracking()
            .OrderBy(e => e.LastName)
            .ThenBy(e => e.FirstName)
            .ToListAsync();

        return View(employees);
    }

    public async Task<IActionResult> Details(int id)
    {
        var employee = await context.Employees
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == id);

        if (employee is null)
        {
            return NotFound();
        }

        var payrollHistory = await context.PayrollRecords
            .AsNoTracking()
            .Where(p => p.EmployeeId == id)
            .OrderByDescending(p => p.Date)
            .ToListAsync();

        return View(new EmployeeDetailsViewModel
        {
            Employee = employee,
            PayrollHistory = payrollHistory
        });
    }

    [Authorize(Roles = "Admin")]
    public IActionResult Create()
    {
        return View(new Employee());
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Employee employee)
    {
        if (!ModelState.IsValid)
        {
            return View(employee);
        }

        context.Employees.Add(employee);
        await context.SaveChangesAsync();

        await WriteAuditAsync("Create", employee.Id, $"Created employee {employee.FullName}.");

        TempData["SuccessMessage"] = "Employee created successfully.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(int id)
    {
        var employee = await context.Employees.FindAsync(id);
        if (employee is null)
        {
            return NotFound();
        }

        return View(employee);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Employee employee)
    {
        if (id != employee.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            return View(employee);
        }

        var existingEmployee = await context.Employees.FindAsync(id);
        if (existingEmployee is null)
        {
            return NotFound();
        }

        existingEmployee.FirstName = employee.FirstName;
        existingEmployee.LastName = employee.LastName;
        existingEmployee.Position = employee.Position;
        existingEmployee.Department = employee.Department;
        existingEmployee.DailyRate = employee.DailyRate;

        await context.SaveChangesAsync();

        await WriteAuditAsync("Edit", existingEmployee.Id, $"Updated employee {existingEmployee.FullName}.");

        TempData["SuccessMessage"] = "Employee updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var employee = await context.Employees
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == id);

        if (employee is null)
        {
            return NotFound();
        }

        return View(employee);
    }

    [HttpPost, ActionName("Delete")]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var employee = await context.Employees.FindAsync(id);
        if (employee is null)
        {
            return NotFound();
        }

        await WriteAuditAsync("Delete", employee.Id, $"Deleted employee {employee.FullName}.");

        context.Employees.Remove(employee);
        await context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Employee deleted successfully.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AuditLogs()
    {
        var logs = await context.AuditLogs
            .AsNoTracking()
            .OrderByDescending(a => a.PerformedAtUtc)
            .Take(500)
            .ToListAsync();

        return View(logs);
    }

    private async Task WriteAuditAsync(string action, int entityId, string details)
    {
        context.AuditLogs.Add(new AuditLog
        {
            Action = action,
            EntityName = "Employee",
            EntityId = entityId,
            PerformedBy = string.IsNullOrWhiteSpace(User.Identity?.Name) ? "Unknown" : User.Identity.Name,
            PerformedAtUtc = DateTime.UtcNow,
            Details = details
        });

        await context.SaveChangesAsync();
    }
}
