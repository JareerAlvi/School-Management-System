using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace School_System.Models
{
    public class TeacherAttendance
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int TeacherId { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Required]

        public bool IsMarked { get; set; }
        public bool? IsPresent { get; set; }

        public string? Remarks { get; set; }

        public virtual Teacher Teacher { get; set; } = null!;
    }
}
