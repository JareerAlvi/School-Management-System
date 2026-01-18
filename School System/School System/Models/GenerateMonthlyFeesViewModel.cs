namespace School_System.Models
{
    public class GenerateMonthlyFeesViewModel
    {
        public List<Fee> Fees { get; set; }

        // Filter selections
        public string FilterStudentName { get; set; }
        public int? FilterDueMonth { get; set; }
        public string FilterPaymentStatus { get; set; }

        // Dropdown lists
        public List<string> AvailableStudentNames { get; set; }
        public List<int> AvailableMonths { get; set; }
        public List<string> AvailablePaymentStatuses { get; set; }
    }

}
