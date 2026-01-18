using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace School_System.Models
{
    public partial class Teacher
    {
        public int TeacherId { get; set; }
        public string? UserId { get; set; } // Add this


        [Required(ErrorMessage = "Full Name is required")]
        public string? FullName { get; set; }


        [Required(ErrorMessage = "Qualification is required")]
        public string? Qualification { get; set; }

        [Required(ErrorMessage = "Contact Info is required")]
        [Phone(ErrorMessage = "Invalid phone number")]
        public string? ContactInfo { get; set; }

        [Required(ErrorMessage = "Hire Date is required")]
        public DateOnly? HireDate { get; set; }

        [Required(ErrorMessage = "Salary is required")]
        [Range(0, 1000000, ErrorMessage = "Salary must be a positive number")]
        public decimal? Salary { get; set; }
        public bool IsPaid { get; set; } = false; // default false
        public bool IsPaymentConfirmed { get; set; } = false;

        public bool? IsHeadTeacher { get; set; } = false;   

        public string ?ClassName { get; set; }=string.Empty;


        public virtual ICollection<TeacherAssignedSubject> TeacherAssignedSubjects { get; set; } = new List<TeacherAssignedSubject>();
        public virtual ICollection<TeacherAttendance> TeacherAttendances { get; set; } = new List<TeacherAttendance>();
        public virtual ICollection<HeadTeacherClasses> HeadTeacherClasses { get; set; } = new List<HeadTeacherClasses>();

        public virtual ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
        public virtual ICollection<Grade> Grades { get; set; } = new List<Grade>();
    }
}
