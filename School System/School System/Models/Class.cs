using System.ComponentModel.DataAnnotations;

namespace School_System.Models
{
    public partial class Class
    {
        public int ClassId { get; set; } // No validation here, database sets this

        [Required(ErrorMessage = "Class Name is required")]
        public string? ClassName { get; set; }

        [Required(ErrorMessage = "Room Number is required")]
        public string? RoomNumber { get; set; }

        [Required(ErrorMessage = "Max Capacity is required")]
        [Range(1, 1000, ErrorMessage = "Max Capacity must be between 1 and 1000")]
        public int? MaxCapacity { get; set; }

        public int? HeadTeacherID { get; set; }

        public string? TeacherName { get; set; }
        public virtual ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
        public virtual ICollection<Subject> Subjects { get; set; } = new List<Subject>();
        public virtual ICollection<Student> Students { get; set; } = new List<Student>();
    }

}
