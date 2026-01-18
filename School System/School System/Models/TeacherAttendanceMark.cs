using School_System.Models;

public class TeacherAttendanceMarkViewModel
{
    public List<Teacher> Teachers { get; set; } = new();
    public List<TeacherAttendance> TodayAttendance { get; set; } = new();
}
