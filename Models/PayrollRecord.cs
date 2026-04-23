using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IT15_LabExam_Bais.Models;

public class PayrollRecord
{
    [Key]
    [Display(Name = "Payroll ID")]
    public int Id { get; set; }

    [Required]
    [Display(Name = "Employee")]
    public int EmployeeId { get; set; }

    [Required]
    [DataType(DataType.Date)]
    public DateTime Date { get; set; }

    [Required]
    [Range(typeof(decimal), "0", "31")]
    [Display(Name = "Days Worked")]
    public decimal DaysWorked { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    [Display(Name = "Gross Pay")]
    public decimal GrossPay { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    [Range(typeof(decimal), "0", "99999999")]
    public decimal Deduction { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    [Display(Name = "Net Pay")]
    public decimal NetPay { get; set; }

    public Employee? Employee { get; set; }
}
