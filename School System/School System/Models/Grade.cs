using System;
using System.Collections.Generic;

namespace School_System.Models;

public partial class Grade
{
    public int GradeId { get; set; }

    public int? StudentId { get; set; }

    public int? SubjectId { get; set; }

    public int? TeacherId { get; set; }

    public int? Marks { get; set; }

    public string? ExamType { get; set; }

    public DateOnly? GradeDate { get; set; }

    public virtual Student? Student { get; set; }

    public virtual Subject? Subject { get; set; }

    public virtual Teacher? Teacher { get; set; }
}
