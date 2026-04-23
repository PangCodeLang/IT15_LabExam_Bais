namespace IT15_LabExam_Bais.Models;

public class StudentsIndexViewModel
{
    public string SearchTerm { get; set; } = string.Empty;
    public string CourseFilter { get; set; } = string.Empty;
    public List<string> Courses { get; set; } = [];
    public List<Student> Students { get; set; } = [];
}
