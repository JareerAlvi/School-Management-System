using System.ComponentModel.DataAnnotations;

namespace School_System.Models
{
    public class TeacherAssignedSubject
    {
        public int Id { get; set; }

        public int TeacherId { get; set; }

        public int SubjectId { get; set; }

        public string SubjectName { get; set; } = string.Empty;
        public string ClassName { get; set; } = string.Empty;
        public bool IsAssigned { get; set; } = true;

        public virtual Teacher Teacher { get; set; } = null!;
        public virtual Subject Subject { get; set; } = null!;
    }

}
