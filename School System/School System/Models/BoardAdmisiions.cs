using School_System.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolManagementSystem.Models
{
    public class BoardAdmission
    {
        [Key]
        public int BoardAdmissionId { get; set; }

        [Required]
        public int StudentId { get; set; }

        [Required]
        public int ClassId { get; set; }  // <-- store the class at the time of admission

        public string? ClassName { get; set; }
        public bool IsSent { get; set; } = false;
        public DateOnly? SentDate { get; set; }       // <- public
        public bool IsRejected { get; set; } = false;
        public bool ReceivedByStudent { get; set; } = false;
        public DateOnly? RecievedDate { get; set; }   // <- public

        public virtual Student Student { get; set; } = null!;
    }

}
