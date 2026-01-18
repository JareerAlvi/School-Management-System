using School_System.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolManagementSystem.Models
{
    public class Certificate
    {
        public int CertificateId { get; set; }

        [Required]
        public int StudentId { get; set; }

        [ForeignKey("StudentId")]
        public Student Student { get; set; } = null!; // Nav property to Student

        // Whether the board has sent the degree
        public bool HasArrived { get; set; } = false;

        public DateOnly? arrivalDte { get; set; }
        // Whether the student has collected it
        public bool HasReceived { get; set; } = false;
        public DateOnly? receivedDate { get; set; }  
        [StringLength(500)]
        public string? Remarks { get; set; }
    }
}
