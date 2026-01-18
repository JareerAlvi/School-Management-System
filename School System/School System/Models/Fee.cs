using System;
using System.Collections.Generic;

namespace School_System.Models;

public partial class Fee
{
    public int FeeId { get; set; }

    public int? StudentId { get; set; }

    public decimal? FeeAmount { get; set; }

    public string? PaymentStatus { get; set; }

    public DateOnly? DueDate { get; set; }

    public DateOnly? PaymentDate { get; set; }

    public string? PaymentMethod { get; set; }

    public virtual Student? Student { get; set; }
}
