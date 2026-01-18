using SchoolManagementSystem.Models;
using System.ComponentModel.DataAnnotations;
namespace School_System.Models;
public partial class Student
{
    public int StudentId { get; set; }

    public int? UserId { get; set; }

    [Required]
    public string? FullName { get; set; }

    [Required]
    public DateOnly? DateOfBirth { get; set; }

    [Required]
    public string? Gender { get; set; }

    [Required]
    [Phone]
    public string? Phone { get; set; }

    [EmailAddress]
    public string? Email { get; set; }

    [Required]
    public string? Address { get; set; }

    public DateOnly? BoardYear { get; set; }
    public string? GuardianInfo { get; set; }

    [Required]
    public DateOnly? AdmissionDate { get; set; }

    [Required]
    public int? ClassId { get; set; }

    public virtual ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();

    public virtual ICollection<Fee> Fees { get; set; } = new List<Fee>();

    public virtual ICollection<Grade> Grades { get; set; } = new List<Grade>();

    public virtual Class? Class { get; set; }

    public virtual ICollection<Certificate> Certificates { get; set; } = new List<Certificate>();
    public virtual ICollection<BoardAdmission> BoardAdmissions { get; set; } = new List<BoardAdmission>();
}
