using ClosedXML.Excel;
using IT15_LabExam_Bais.Data;
using IT15_LabExam_Bais.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IT15_LabExam_Bais.Controllers;

[Authorize(Roles = "Admin,Staff")]
public class StudentsController(ApplicationDbContext context) : Controller
{
    public async Task<IActionResult> Index(string? searchTerm, string? courseFilter)
    {
        IQueryable<Student> query = context.Students.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var likeTerm = $"%{searchTerm.Trim()}%";
            query = query
                .Where(s =>
                    EF.Functions.Like(s.StudentNumber, likeTerm)
                    || EF.Functions.Like(s.FirstName, likeTerm)
                    || EF.Functions.Like(s.LastName, likeTerm)
                    || EF.Functions.Like(s.Email, likeTerm)
                    || EF.Functions.Like(s.Course, likeTerm));
        }

        if (!string.IsNullOrWhiteSpace(courseFilter))
        {
            query = query.Where(s => s.Course == courseFilter);
        }

        var students = await query
            .OrderBy(s => s.LastName)
            .ThenBy(s => s.FirstName)
            .ToListAsync();

        var viewModel = new StudentsIndexViewModel
        {
            SearchTerm = searchTerm ?? string.Empty,
            CourseFilter = courseFilter ?? string.Empty,
            Courses = await context.Students
                .Select(s => s.Course)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync(),
            Students = students
        };

        return View(viewModel);
    }

    public async Task<IActionResult> Details(int id)
    {
        var student = await context.Students.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id);
        if (student is null)
        {
            return NotFound();
        }

        return View(student);
    }

    [Authorize(Roles = "Admin")]
    public IActionResult Create()
    {
        return View(new Student
        {
            BirthDate = DateOnly.FromDateTime(DateTime.Today.AddYears(-18)),
            YearLevel = 1
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create(Student student)
    {
        var studentNumberExists = await context.Students
            .AnyAsync(s => s.StudentNumber == student.StudentNumber);

        if (studentNumberExists)
        {
            ModelState.AddModelError(nameof(Student.StudentNumber), "Student number already exists.");
        }

        if (!ModelState.IsValid)
        {
            return View(student);
        }

        context.Students.Add(student);
        await context.SaveChangesAsync();

        await LogAuditAsync(
            action: "Create",
            entityId: student.Id,
            details: $"Created student {student.StudentNumber} ({student.FullName}).");

        TempData["SuccessMessage"] = "Student created successfully.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(int id)
    {
        var student = await context.Students.FindAsync(id);
        if (student is null)
        {
            return NotFound();
        }

        return View(student);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(int id, Student student)
    {
        if (id != student.Id)
        {
            return BadRequest();
        }

        var studentNumberExists = await context.Students
            .AnyAsync(s => s.StudentNumber == student.StudentNumber && s.Id != student.Id);

        if (studentNumberExists)
        {
            ModelState.AddModelError(nameof(Student.StudentNumber), "Student number already exists.");
        }

        if (!ModelState.IsValid)
        {
            return View(student);
        }

        var existingStudent = await context.Students.FindAsync(id);
        if (existingStudent is null)
        {
            return NotFound();
        }

        existingStudent.StudentNumber = student.StudentNumber;
        existingStudent.FirstName = student.FirstName;
        existingStudent.LastName = student.LastName;
        existingStudent.Email = student.Email;
        existingStudent.Course = student.Course;
        existingStudent.YearLevel = student.YearLevel;
        existingStudent.BirthDate = student.BirthDate;

        await context.SaveChangesAsync();

        await LogAuditAsync(
            action: "Edit",
            entityId: existingStudent.Id,
            details: $"Updated student {existingStudent.StudentNumber} ({existingStudent.FullName}).");

        TempData["SuccessMessage"] = "Student updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var student = await context.Students.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id);
        if (student is null)
        {
            return NotFound();
        }

        return View(student);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var student = await context.Students.FindAsync(id);
        if (student is null)
        {
            return NotFound();
        }

        await LogAuditAsync(
            action: "Delete",
            entityId: student.Id,
            details: $"Deleted student {student.StudentNumber} ({student.FullName}).");

        context.Students.Remove(student);
        await context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Student deleted successfully.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Print(string? searchTerm, string? courseFilter)
    {
        var students = await ApplyFiltersAsync(searchTerm, courseFilter);
        return View(students);
    }

    public async Task<FileResult> Export(string? searchTerm, string? courseFilter)
    {
        var students = await ApplyFiltersAsync(searchTerm, courseFilter);

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Students");

        worksheet.Cell(1, 1).Value = "Student Number";
        worksheet.Cell(1, 2).Value = "First Name";
        worksheet.Cell(1, 3).Value = "Last Name";
        worksheet.Cell(1, 4).Value = "Email";
        worksheet.Cell(1, 5).Value = "Course";
        worksheet.Cell(1, 6).Value = "Year Level";
        worksheet.Cell(1, 7).Value = "Birth Date";

        for (var i = 0; i < students.Count; i++)
        {
            var row = i + 2;
            var student = students[i];
            worksheet.Cell(row, 1).Value = student.StudentNumber;
            worksheet.Cell(row, 2).Value = student.FirstName;
            worksheet.Cell(row, 3).Value = student.LastName;
            worksheet.Cell(row, 4).Value = student.Email;
            worksheet.Cell(row, 5).Value = student.Course;
            worksheet.Cell(row, 6).Value = student.YearLevel;
            worksheet.Cell(row, 7).Value = student.BirthDate.ToDateTime(TimeOnly.MinValue);
            worksheet.Cell(row, 7).Style.DateFormat.Format = "yyyy-mm-dd";
        }

        var headerRange = worksheet.Range(1, 1, 1, 7);
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);

        return File(
            stream.ToArray(),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"students-{DateTime.Now:yyyyMMddHHmmss}.xlsx");
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AuditLogs()
    {
        var logs = await context.AuditLogs
            .AsNoTracking()
            .OrderByDescending(l => l.PerformedAtUtc)
            .Take(300)
            .ToListAsync();

        return View(logs);
    }

    private async Task<List<Student>> ApplyFiltersAsync(string? searchTerm, string? courseFilter)
    {
        IQueryable<Student> query = context.Students.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var likeTerm = $"%{searchTerm.Trim()}%";
            query = query.Where(s =>
                EF.Functions.Like(s.StudentNumber, likeTerm)
                || EF.Functions.Like(s.FirstName, likeTerm)
                || EF.Functions.Like(s.LastName, likeTerm)
                || EF.Functions.Like(s.Email, likeTerm)
                || EF.Functions.Like(s.Course, likeTerm));
        }

        if (!string.IsNullOrWhiteSpace(courseFilter))
        {
            query = query.Where(s => s.Course == courseFilter);
        }

        return await query
            .OrderBy(s => s.LastName)
            .ThenBy(s => s.FirstName)
            .ToListAsync();
    }

    private async Task LogAuditAsync(string action, int entityId, string details)
    {
        var userEmail = User?.Identity?.Name;
        context.AuditLogs.Add(new AuditLog
        {
            Action = action,
            EntityName = "Student",
            EntityId = entityId,
            PerformedBy = string.IsNullOrWhiteSpace(userEmail) ? "Unknown" : userEmail,
            PerformedAtUtc = DateTime.UtcNow,
            Details = details
        });

        await context.SaveChangesAsync();
    }
}
