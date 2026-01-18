namespace School_System.Models
{
    public class TeacherViewModel
    {
        public int TeacherId { get; set; }
        public string FullName { get; set; } = null!;
        public string Qualification { get; set; } = null!;
        public string ContactInfo { get; set; } = null!;
        public DateTime HireDate { get; set; }
        public decimal Salary { get; set; }
        public string Email { get; set; } = "N/A";
        public bool IsHeadTeacher { get; set; }

    }

}
