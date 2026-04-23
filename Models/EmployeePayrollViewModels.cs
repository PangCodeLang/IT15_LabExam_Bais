namespace IT15_LabExam_Bais.Models;

public class EmployeeDetailsViewModel
{
    public Employee Employee { get; set; } = new();
    public List<PayrollRecord> PayrollHistory { get; set; } = [];
}

public class PayrollIndexViewModel
{
    public int? EmployeeId { get; set; }
    public List<Employee> Employees { get; set; } = [];
    public List<PayrollRecord> PayrollRecords { get; set; } = [];
}
