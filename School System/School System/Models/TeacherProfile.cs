using School_System.Models;

public class TeacherProfileViewModel
{
    public int TeacherId { get; set; }
    public string FullName { get; set; } = "";
    public string Email { get; set; } = "";
    public string Qualification { get; set; } = "";
    public string ContactInfo { get; set; } = "";
    public DateOnly HireDate { get; set; }
    public decimal Salary { get; set; }
    public bool IsPaid { get; set; }
    public bool IsPaymentConfirmed { get; set; }
    public List<string> HeadTeacherClasses { get; set; } = new();

    public List<SubjectViewModel> Subjects { get; set; } = new List<SubjectViewModel>();

    public List<SubjectViewModel> UnassignedSubjects { get; set; } = new List<SubjectViewModel>();

    // NEW - List of all classes for the dropdown
    public List<ClassViewModel> AllClasses { get; set; } = new List<ClassViewModel>();
}

public class SubjectViewModel
{
    public int Id { get; set; } // TeacherAssignedSubject PK
    public int SubjectId { get; set; }
    public string SubjectName { get; set; }
    public string ClassName { get; set; }
}


public class AddSubjectViewModel
{
    public int TeacherId { get; set; }
    public List<Subject> Subjects { get; set; } = new();
    public int? SelectedSubjectId { get; set; }
    public int? SelectedClassId { get; set; }  // Optional if you want class selection
}
public class ClassViewModel
{
    public int ClassId { get; set; }
    public string ClassName { get; set; } = "";
}
