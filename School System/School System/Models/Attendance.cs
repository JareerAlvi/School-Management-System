using System;
using System.Collections.Generic;

namespace School_System.Models;

public partial class Attendance
{
    public int AttendanceId { get; set; }

    public DateOnly? Date { get; set; }

    public int? ClassId { get; set; }

    public int? StudentId { get; set; }

    public string? Status { get; set; }

    public int? TeacherId { get; set; }

    public virtual Class? Class { get; set; }

    public virtual Student? Student { get; set; }

    public virtual Teacher? Teacher { get; set; }
}
