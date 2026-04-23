using IT15_LabExam_Bais.Data;
using IT15_LabExam_Bais.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace IT15_LabExam_Bais.Controllers;

[Authorize(Roles = "Admin,Staff")]
public class PayrollController(ApplicationDbContext context) : Controller
{
    public async Task<IActionResult> Index(int? employeeId)
    {
        var employees = await context.Employees
            .AsNoTracking()
            .OrderBy(e => e.LastName)
            .ThenBy(e => e.FirstName)
            .ToListAsync();

        var query = context.PayrollRecords
            .AsNoTracking()
            .Include(p => p.Employee)
            .AsQueryable();

        if (employeeId.HasValue)
        {
            query = query.Where(p => p.EmployeeId == employeeId.Value);
        }

        var records = await query
            .OrderByDescending(p => p.Date)
            .ToListAsync();

        return View(new PayrollIndexViewModel
        {
            EmployeeId = employeeId,
            Employees = employees,
            PayrollRecords = records
        });
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create(int? employeeId)
    {
        await PopulateEmployeesDropdown(employeeId);

        return View(new PayrollRecord
        {
            Date = DateTime.Today,
            EmployeeId = employeeId ?? 0,
            DaysWorked = 0,
            Deduction = 0
        });
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PayrollRecord payrollRecord)
    {
        var employee = await context.Employees.FindAsync(payrollRecord.EmployeeId);
        if (employee is null)
        {
            ModelState.AddModelError(nameof(PayrollRecord.EmployeeId), "Selected employee does not exist.");
        }

        if (employee is not null)
        {
            payrollRecord.GrossPay = payrollRecord.DaysWorked * employee.DailyRate;
            payrollRecord.NetPay = payrollRecord.GrossPay - payrollRecord.Deduction;

            if (payrollRecord.Deduction > payrollRecord.GrossPay)
            {
                ModelState.AddModelError(nameof(PayrollRecord.Deduction), "Deduction cannot exceed gross pay.");
            }
        }

        if (!ModelState.IsValid)
        {
            await PopulateEmployeesDropdown(payrollRecord.EmployeeId);
            return View(payrollRecord);
        }

        context.PayrollRecords.Add(payrollRecord);
        await context.SaveChangesAsync();

        await WriteAuditAsync(
            "Create",
            payrollRecord.Id,
            $"Created payroll record for employee ID {payrollRecord.EmployeeId} (Gross: {payrollRecord.GrossPay:F2}, Net: {payrollRecord.NetPay:F2}).");

        TempData["SuccessMessage"] = "Payroll record created successfully.";
        return RedirectToAction(nameof(Index), new { employeeId = payrollRecord.EmployeeId });
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(int id)
    {
        var payrollRecord = await context.PayrollRecords.FindAsync(id);
        if (payrollRecord is null)
        {
            return NotFound();
        }

        await PopulateEmployeesDropdown(payrollRecord.EmployeeId);
        return View(payrollRecord);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, PayrollRecord payrollRecord)
    {
        if (id != payrollRecord.Id)
        {
            return BadRequest();
        }

        var existingRecord = await context.PayrollRecords.FindAsync(id);
        if (existingRecord is null)
        {
            return NotFound();
        }

        var employee = await context.Employees.FindAsync(payrollRecord.EmployeeId);
        if (employee is null)
        {
            ModelState.AddModelError(nameof(PayrollRecord.EmployeeId), "Selected employee does not exist.");
        }

        if (employee is not null)
        {
            payrollRecord.GrossPay = payrollRecord.DaysWorked * employee.DailyRate;
            payrollRecord.NetPay = payrollRecord.GrossPay - payrollRecord.Deduction;

            if (payrollRecord.Deduction > payrollRecord.GrossPay)
            {
                ModelState.AddModelError(nameof(PayrollRecord.Deduction), "Deduction cannot exceed gross pay.");
            }
        }

        if (!ModelState.IsValid)
        {
            await PopulateEmployeesDropdown(payrollRecord.EmployeeId);
            return View(payrollRecord);
        }

        existingRecord.EmployeeId = payrollRecord.EmployeeId;
        existingRecord.Date = payrollRecord.Date;
        existingRecord.DaysWorked = payrollRecord.DaysWorked;
        existingRecord.Deduction = payrollRecord.Deduction;
        existingRecord.GrossPay = payrollRecord.GrossPay;
        existingRecord.NetPay = payrollRecord.NetPay;

        await context.SaveChangesAsync();

        await WriteAuditAsync(
            "Edit",
            existingRecord.Id,
            $"Updated payroll record for employee ID {existingRecord.EmployeeId} (Gross: {existingRecord.GrossPay:F2}, Net: {existingRecord.NetPay:F2}).");

        TempData["SuccessMessage"] = "Payroll record updated successfully.";
        return RedirectToAction(nameof(Index), new { employeeId = existingRecord.EmployeeId });
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var payrollRecord = await context.PayrollRecords
            .AsNoTracking()
            .Include(p => p.Employee)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (payrollRecord is null)
        {
            return NotFound();
        }

        return View(payrollRecord);
    }

    [HttpPost, ActionName("Delete")]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var payrollRecord = await context.PayrollRecords.FindAsync(id);
        if (payrollRecord is null)
        {
            return NotFound();
        }

        var employeeId = payrollRecord.EmployeeId;

        await WriteAuditAsync(
            "Delete",
            payrollRecord.Id,
            $"Deleted payroll record for employee ID {payrollRecord.EmployeeId}.");

        context.PayrollRecords.Remove(payrollRecord);
        await context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Payroll record deleted successfully.";
        return RedirectToAction(nameof(Index), new { employeeId });
    }

    private async Task PopulateEmployeesDropdown(int? selectedEmployeeId = null)
    {
        var employees = await context.Employees
            .AsNoTracking()
            .OrderBy(e => e.LastName)
            .ThenBy(e => e.FirstName)
            .Select(e => new { e.Id, Name = e.LastName + ", " + e.FirstName })
            .ToListAsync();

        ViewBag.EmployeeId = new SelectList(employees, "Id", "Name", selectedEmployeeId);
    }

    private async Task WriteAuditAsync(string action, int entityId, string details)
    {
        context.AuditLogs.Add(new AuditLog
        {
            Action = action,
            EntityName = "PayrollRecord",
            EntityId = entityId,
            PerformedBy = string.IsNullOrWhiteSpace(User.Identity?.Name) ? "Unknown" : User.Identity.Name,
            PerformedAtUtc = DateTime.UtcNow,
            Details = details
        });

        await context.SaveChangesAsync();
    }
}
