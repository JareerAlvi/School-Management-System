using System;
using System.Collections.Generic;

namespace School_System.Models;

public partial class DailyAnalytic
{
    public int AnalyticsId { get; set; }

    public DateOnly? Date { get; set; }

    public int? TotalStudents { get; set; }

    public int? TotalTeachers { get; set; }

    public int? TotalParents { get; set; }

    public double? AttendancePercentage { get; set; }

    public decimal? FeeCollected { get; set; }

    public double? GradeAverage { get; set; }
}
