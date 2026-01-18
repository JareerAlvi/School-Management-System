using System.ComponentModel.DataAnnotations;

namespace School_System.Models
{
    public class HeadTeacherClasses
    {
        public int Id { get; set; } // PK for this table

        public int TeacherId { get; set; }


        public string? ClassName { get; set; } 

        // Navigation property
        public virtual Teacher Teacher { get; set; } = null!;
    }
}
