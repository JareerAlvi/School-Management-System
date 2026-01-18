using System.ComponentModel.DataAnnotations;

namespace School_System
{

    public class TeacherCreateViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = null!;

        [Required]
        public string FullName { get; set; } = null!;

        [Required]
        public string Qualification { get; set; } = null!;

        [Required]
        public string ContactInfo { get; set; } = null!;

        [Required]
        public DateTime HireDate { get; set; }

        [Required]
        [Range(0, 1000000)]
        public decimal Salary { get; set; }
    }

}
