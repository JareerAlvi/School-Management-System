
using System.ComponentModel.DataAnnotations;
namespace School_System.Models
{
    public partial class Subject
    {
        public int SubjectId { get; set; }

        [Required(ErrorMessage = "Please select a class.")]
        public int? ClassId { get; set; }

        [Required(ErrorMessage = "Subject name is required.")]
        public string? SubjectName { get; set; }

        public bool IsAssigned { get; set; } = false;

        public virtual ICollection<TeacherAssignedSubject> TeacherAssignedSubjects { get; set; } = new List<TeacherAssignedSubject>();

        public virtual Class Class { get; set; } = null!;

        public virtual ICollection<Grade> Grades { get; set; } = new List<Grade>();
    }
}

