using System.ComponentModel.DataAnnotations;

namespace IT15_LabExam_Bais.Models;

public class Student
{
    public int Id { get; set; }

    [Required]
    [Display(Name = "Student Number")]
    [StringLength(20, MinimumLength = 5)]
    public string StudentNumber { get; set; } = string.Empty;

    [Required]
    [Display(Name = "First Name")]
    [StringLength(50)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Last Name")]
    [StringLength(50)]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(100)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Course { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Year Level")]
    [Range(1, 5)]
    public int YearLevel { get; set; }

    [Required]
    [DataType(DataType.Date)]
    [Display(Name = "Birth Date")]
    public DateOnly BirthDate { get; set; }

    public string FullName => $"{LastName}, {FirstName}";
}
