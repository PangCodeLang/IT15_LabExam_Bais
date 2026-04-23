using IT15_LabExam_Bais.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace IT15_LabExam_Bais.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext(options)
{
    public DbSet<Student> Students => Set<Student>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Student>()
            .HasIndex(s => s.StudentNumber)
            .IsUnique();

        builder.Entity<Student>()
            .HasIndex(s => s.Email)
            .IsUnique();

        builder.Entity<AuditLog>()
            .HasIndex(a => a.PerformedAtUtc);

        builder.Entity<AuditLog>()
            .HasIndex(a => new { a.EntityName, a.EntityId });
    }
}
