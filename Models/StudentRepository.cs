namespace IT15_LabExam_Bais.Models;

public static class StudentRepository
{
    private static readonly object LockObject = new();
    private static readonly List<Student> Students =
    [
        new Student
        {
            Id = 1,
            StudentNumber = "2026-0001",
            FirstName = "Andrea",
            LastName = "Cruz",
            Email = "andrea.cruz@example.com",
            Course = "BS Information Technology",
            YearLevel = 2,
            BirthDate = new DateOnly(2005, 3, 12)
        },
        new Student
        {
            Id = 2,
            StudentNumber = "2026-0002",
            FirstName = "Marco",
            LastName = "Santos",
            Email = "marco.santos@example.com",
            Course = "BS Computer Science",
            YearLevel = 3,
            BirthDate = new DateOnly(2004, 10, 4)
        },
        new Student
        {
            Id = 3,
            StudentNumber = "2026-0003",
            FirstName = "Leah",
            LastName = "Garcia",
            Email = "leah.garcia@example.com",
            Course = "BS Information Systems",
            YearLevel = 1,
            BirthDate = new DateOnly(2006, 7, 28)
        }
    ];

    public static List<Student> GetAll()
    {
        lock (LockObject)
        {
            return Students
                .Select(Clone)
                .ToList();
        }
    }

    public static Student? GetById(int id)
    {
        lock (LockObject)
        {
            var student = Students.FirstOrDefault(s => s.Id == id);
            return student is null ? null : Clone(student);
        }
    }

    public static bool StudentNumberExists(string studentNumber, int? excludeId = null)
    {
        lock (LockObject)
        {
            return Students.Any(s =>
                s.StudentNumber.Equals(studentNumber, StringComparison.OrdinalIgnoreCase)
                && (!excludeId.HasValue || s.Id != excludeId.Value));
        }
    }

    public static void Add(Student student)
    {
        lock (LockObject)
        {
            var nextId = Students.Count == 0 ? 1 : Students.Max(s => s.Id) + 1;
            student.Id = nextId;
            Students.Add(Clone(student));
        }
    }

    public static bool Update(Student student)
    {
        lock (LockObject)
        {
            var index = Students.FindIndex(s => s.Id == student.Id);
            if (index < 0)
            {
                return false;
            }

            Students[index] = Clone(student);
            return true;
        }
    }

    public static bool Delete(int id)
    {
        lock (LockObject)
        {
            var student = Students.FirstOrDefault(s => s.Id == id);
            if (student is null)
            {
                return false;
            }

            Students.Remove(student);
            return true;
        }
    }

    public static List<string> GetCourses()
    {
        lock (LockObject)
        {
            return Students
                .Select(s => s.Course)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(c => c)
                .ToList();
        }
    }

    private static Student Clone(Student student)
    {
        return new Student
        {
            Id = student.Id,
            StudentNumber = student.StudentNumber,
            FirstName = student.FirstName,
            LastName = student.LastName,
            Email = student.Email,
            Course = student.Course,
            YearLevel = student.YearLevel,
            BirthDate = student.BirthDate
        };
    }
}
