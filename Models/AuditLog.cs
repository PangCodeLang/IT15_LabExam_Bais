using System.ComponentModel.DataAnnotations;

namespace IT15_LabExam_Bais.Models;

public class AuditLog
{
    public int Id { get; set; }

    [Required]
    [StringLength(40)]
    public string Action { get; set; } = string.Empty;

    [Required]
    [StringLength(60)]
    public string EntityName { get; set; } = string.Empty;

    public int EntityId { get; set; }

    [Required]
    [StringLength(150)]
    public string PerformedBy { get; set; } = string.Empty;

    [Required]
    public DateTime PerformedAtUtc { get; set; }

    [StringLength(1000)]
    public string Details { get; set; } = string.Empty;
}
