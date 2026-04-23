using IT15_LabExam_Bais.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace IT15_LabExam_Bais.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext(options)
{
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<PayrollRecord> PayrollRecords => Set<PayrollRecord>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Employee>()
            .HasIndex(e => new { e.LastName, e.FirstName });

        builder.Entity<PayrollRecord>()
            .HasOne(p => p.Employee)
            .WithMany(e => e.PayrollRecords)
            .HasForeignKey(p => p.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<PayrollRecord>()
            .HasIndex(p => new { p.EmployeeId, p.Date });

        builder.Entity<AuditLog>()
            .HasIndex(a => a.PerformedAtUtc);

        builder.Entity<AuditLog>()
            .HasIndex(a => new { a.EntityName, a.EntityId });
    }
}
