using System.ComponentModel.DataAnnotations;

public class StudentCreateViewModel
{
    public int StudentId { get; set; }
    [Required]
    public int? ClassId { get; set; }
    [Required]
    public string FullName { get; set; }

    

    [Required]
    public DateOnly? DateOfBirth { get; set; }

    [Required]
    public string Gender { get; set; }

    [Required]
    public string Phone { get; set; }

    [EmailAddress]
    public string? Email { get; set; }

    [Required]
    public string Address { get; set; }
    [Required]
    public string GuardianName { get; set; }

    [Required]
    public DateOnly? AdmissionDate { get; set; }


    [Required]
    [Range(0, double.MaxValue, ErrorMessage = "Fee must be a positive value.")]
    public decimal FeeAmount { get; set; }

    public string? PaymentStatus { get; set; }

 
    public DateOnly? FeeDueDate { get; set; }

    public string? ClassName { get; set; } = null;
}