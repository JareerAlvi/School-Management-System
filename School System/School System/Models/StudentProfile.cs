using School_System.Models;

public class StudentProfileViewModel
{
    public int StudentId { get; set; }
    public string FullName { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public string Gender { get; set; }
    public string Phone { get; set; }
    public string Email { get; set; }
    public string Address { get; set; }
    public string GuardianName { get; set; }
    public DateOnly? AdmissionDate { get; set; }
    public string ClassName { get; set; }

    public decimal? LastFeeAmount { get; set; }
    public DateOnly? LastFeeDueDate { get; set; }
    public string LastFeeStatus { get; set; }
    public Student Student { get; set; }
    public List<Fee> Fees { get; set; }
}
