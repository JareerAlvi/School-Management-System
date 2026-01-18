using System.ComponentModel.DataAnnotations;

public class TeacherEditViewModel
{
    public int TeacherId { get; set; }

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress]
    public string Email { get; set; }

    public string? Password { get; set; } // Optional

    [Required]
    public string FullName { get; set; }

    [Required]
    public string Qualification { get; set; }

    [Required]
    [Phone]
    public string ContactInfo { get; set; }

    [Required]
    public DateOnly? HireDate { get; set; }

    [Required]
    public decimal? Salary { get; set; }

    public string? OldEmail { get; set; } // For user matching
}
