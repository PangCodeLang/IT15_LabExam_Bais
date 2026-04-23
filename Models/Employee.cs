using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IT15_LabExam_Bais.Models;

public class Employee
{
    [Key]
    [Display(Name = "Employee ID")]
    public int Id { get; set; }

    [Required]
    [StringLength(60)]
    [Display(Name = "First Name")]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [StringLength(60)]
    [Display(Name = "Last Name")]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [StringLength(80)]
    public string Position { get; set; } = string.Empty;

    [Required]
    [StringLength(80)]
    public string Department { get; set; } = string.Empty;

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    [Range(typeof(decimal), "0", "99999999")]
    [Display(Name = "Daily Rate")]
    public decimal DailyRate { get; set; }

    public ICollection<PayrollRecord> PayrollRecords { get; set; } = new List<PayrollRecord>();

    public string FullName => $"{LastName}, {FirstName}";
}
