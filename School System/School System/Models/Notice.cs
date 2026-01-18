using System;
using System.Collections.Generic;

namespace School_System.Models;

public partial class Notice
{
    public int NoticeId { get; set; }

    public string? Title { get; set; }

    public string? Description { get; set; }

    public string? Audience { get; set; }

    public string? AudienceValue { get; set; }

    public int? PostedBy { get; set; }

    public DateOnly? PostDate { get; set; }

    public DateOnly? ExpiryDate { get; set; }

    public virtual User? PostedByNavigation { get; set; }
}
