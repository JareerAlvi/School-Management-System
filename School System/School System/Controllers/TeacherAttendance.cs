using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using School_System.Models;

namespace School_System.Controllers
{
    [Authorize]
    public class TeacherAttendanceController : Controller
    {
        private readonly SchoolManagementSystemContext _context;

        public TeacherAttendanceController(SchoolManagementSystemContext context)
        {
            _context = context;
        }

  
        public async Task<IActionResult> Index(int? teacherId)
        {
            var today = DateTime.Today;

            var allTeachers = await _context.Teachers.ToListAsync();

            // Step 1: Fetch today's attendances
            var todaysAttendances = await _context.TeacherAttendances
                .Where(a => a.Date == today)
                .ToListAsync();

            // Step 2: Get teacher IDs already added for today
            var markedTeacherIds = todaysAttendances.Select(a => a.TeacherId).ToHashSet();

            // Step 3: Add Unmarked entries for teachers not added yet
            var newUnmarkedTeachers = allTeachers
                .Where(t => !markedTeacherIds.Contains(t.TeacherId))
                .ToList();

            if (DateTime.Today.DayOfWeek == DayOfWeek.Sunday)
            {
                foreach (var teacher in newUnmarkedTeachers)
                {
                    _context.TeacherAttendances.Add(new TeacherAttendance
                    {
                        TeacherId = teacher.TeacherId,
                        Date = today,
                        IsMarked = false,       // Not marked yet
                        IsPresent = null,       // Unknown until marked
                        Remarks = "Sunday"
                    });
                }
            }
            else {
                foreach (var teacher in newUnmarkedTeachers)
                {
                    _context.TeacherAttendances.Add(new TeacherAttendance
                    {
                        TeacherId = teacher.TeacherId,
                        Date = today,
                        IsMarked = false,       // Not marked yet
                        IsPresent = null,       // Unknown until marked
                        Remarks = "Unmarked"
                    });
                }
            }
               

            if (newUnmarkedTeachers.Any())
                await _context.SaveChangesAsync();

            // Step 4: Fetch all attendance records, filtered if needed
            var query = _context.TeacherAttendances
                .Include(a => a.Teacher)
                .OrderByDescending(a => a.Date)
                .AsQueryable();

            if (teacherId.HasValue)
                query = query.Where(a => a.TeacherId == teacherId.Value);

            var attendanceList = await query.ToListAsync();

            ViewBag.Teachers = allTeachers;
            ViewBag.SelectedTeacherId = teacherId;

            return View(attendanceList);
        }

        [HttpGet]
        public async Task<IActionResult> Mark()
        {
            var today = DateTime.Today;

            var allTeachers = await _context.Teachers.ToListAsync();
            var todaysAttendance = await _context.TeacherAttendances
                .Where(a => a.Date.Date == today)
                .Include(a => a.Teacher)
                .ToListAsync();

            var vm = new TeacherAttendanceMarkViewModel
            {
                Teachers = allTeachers,
                TodayAttendance = todaysAttendance
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Mark(IFormCollection? form, int? teacherId, bool? isPresent, DateTime? date)
        {
            var targetDate = date?.Date ?? DateTime.Today;
            var now = DateTime.Now;

            string daySuffix = (now.Day % 10 == 1 && now.Day != 11) ? "st" :
                               (now.Day % 10 == 2 && now.Day != 12) ? "nd" :
                               (now.Day % 10 == 3 && now.Day != 13) ? "rd" : "th";

            if (teacherId.HasValue && isPresent.HasValue)
            {
                // Quick mark from menu
                var attendance = await _context.TeacherAttendances
                    .FirstOrDefaultAsync(a => a.TeacherId == teacherId.Value && a.Date.Date == targetDate);

                if (attendance == null)
                {
                    attendance = new TeacherAttendance
                    {
                        TeacherId = teacherId.Value,
                        Date = targetDate,
                        IsPresent = isPresent.Value,
                        IsMarked = true,
                        Remarks = $"Marked at {now:hh:mm tt} {now.Day}{daySuffix} {now:MMMM yyyy}"
                    };
                    _context.TeacherAttendances.Add(attendance);
                }
                else
                {
                    attendance.IsPresent = isPresent.Value;
                    attendance.IsMarked = true;
                    attendance.Remarks = $"Marked at {now:hh:mm tt} {now.Day}{daySuffix} {now:MMMM yyyy}";
                    _context.TeacherAttendances.Update(attendance);
                }

                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            else if (form != null)
            {
                // Bulk mark from form
                var teacherIds = await _context.Teachers.Select(t => t.TeacherId).ToListAsync();

                foreach (var id in teacherIds)
                {
                    var key = $"attendance_{id}";

                    var attendance = await _context.TeacherAttendances
                        .FirstOrDefaultAsync(a => a.TeacherId == id && a.Date.Date == targetDate);

                    if (form.ContainsKey(key))
                    {
                        var present = form[key] == "true";

                        if (attendance == null)
                        {
                            attendance = new TeacherAttendance
                            {
                                TeacherId = id,
                                Date = targetDate,
                                IsPresent = present,
                                IsMarked = true,
                                Remarks = $"Marked at {now:hh:mm tt} {now.Day}{daySuffix} {now:MMMM yyyy}"
                            };
                            _context.TeacherAttendances.Add(attendance);
                        }
                        else
                        {
                            attendance.IsPresent = present;
                            attendance.IsMarked = true;
                            attendance.Remarks = $"Marked at {now:hh:mm tt} {now.Day}{daySuffix} {now:MMMM yyyy}";
                            _context.TeacherAttendances.Update(attendance);
                        }
                    }
                    else
                    {
                        if (attendance == null)
                        {
                            attendance = new TeacherAttendance
                            {
                                TeacherId = id,
                                Date = targetDate,
                                IsPresent = null,
                                IsMarked = false,
                                Remarks = "Unmarked"
                            };
                            _context.TeacherAttendances.Add(attendance);
                        }
                    }
                }

                await _context.SaveChangesAsync();

                var allTeachers = await _context.Teachers.ToListAsync();
                var todaysAttendance = await _context.TeacherAttendances
                    .Where(a => a.Date.Date == targetDate)
                    .Include(a => a.Teacher)
                    .ToListAsync();

                var vm = new TeacherAttendanceMarkViewModel
                {
                    Teachers = allTeachers,
                    TodayAttendance = todaysAttendance
                };

                return View(vm);
            }

            return RedirectToAction("Mark");
        }


        public async Task<IActionResult> Filter(int? teacherId, int? month, string status, string origin)
        {
            var query = _context.TeacherAttendances
                                .Include(a => a.Teacher)
                                .AsQueryable();

            if (origin == "IndividualAttendance")
            {
                // Force teacherId filter, because this view shows only one teacher's data
                if (teacherId.HasValue)
                    query = query.Where(a => a.TeacherId == teacherId.Value);
                else
                    return BadRequest("Teacher ID is required for Individual Attendance.");
            }
            else
            {
                // For bulk view, teacherId is optional
                if (teacherId.HasValue)
                    query = query.Where(a => a.TeacherId == teacherId.Value);
            }

            if (month.HasValue)
                query = query.Where(a => a.Date.Month == month.Value);

            if (!string.IsNullOrEmpty(status))
            {
                switch (status.ToLower())
                {
                    case "present":
                        query = query.Where(a => a.IsPresent == true);
                        break;
                    case "absent":
                        query = query.Where(a => a.IsPresent == false);
                        break;
                    case "marked":
                        query = query.Where(a => a.IsMarked == true);
                        break;
                    case "unmarked":
                        query = query.Where(a => a.IsMarked == false);
                        break;
                }
            }

            var attendance = await query.OrderByDescending(a => a.Date).ToListAsync();

            ViewBag.Teachers = await _context.Teachers.ToListAsync();
            ViewBag.SelectedTeacherId = teacherId;
            ViewBag.SelectedMonth = month;
            ViewBag.SelectedStatus = status;

            if (origin == "IndividualAttendance")
            {
                ViewBag.TeacherId = teacherId;
                return View("IndividualAttendance", attendance);
            }

            return View("Index", attendance);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> QuickMark(int teacherId, bool isPresent, DateTime date, string origin)
        {
            var targetDate = date.Date;
            var now = DateTime.Now;

            string daySuffix = (now.Day % 10 == 1 && now.Day != 11) ? "st" :
                               (now.Day % 10 == 2 && now.Day != 12) ? "nd" :
                               (now.Day % 10 == 3 && now.Day != 13) ? "rd" : "th";

            var attendance = await _context.TeacherAttendances
                .FirstOrDefaultAsync(a => a.TeacherId == teacherId && a.Date.Date == targetDate);

            if (attendance == null)
            {
                attendance = new TeacherAttendance
                {
                    TeacherId = teacherId,
                    Date = targetDate,
                    IsPresent = isPresent,
                    IsMarked = true,
                    Remarks = $"Marked at {now:hh:mm tt} {now.Day}{daySuffix} {now:MMMM yyyy}"
                };
                _context.TeacherAttendances.Add(attendance);
            }
            else
            {
                attendance.IsPresent = isPresent;
                attendance.IsMarked = true;
                attendance.Remarks = $"Marked at {now:hh:mm tt} {now.Day}{daySuffix} {now:MMMM yyyy}";
                _context.TeacherAttendances.Update(attendance);
            }

            await _context.SaveChangesAsync();

            // Redirect back based on origin
            if (origin == "IndividualAttendance")
                return RedirectToAction("IndividualAttendance", new { teacherId = teacherId });

            // default to bulk Index
            return RedirectToAction("Index");
        }


        public async Task<IActionResult> IndividualAttendance(int teacherId)
        {
            var teacherExists = await _context.Teachers.AnyAsync(t => t.TeacherId == teacherId);
            if (!teacherExists) return NotFound();

            var attendanceRecords = await _context.TeacherAttendances
                .Include(a => a.Teacher)   // <--- Include Teacher here
                .Where(a => a.TeacherId == teacherId)
                .OrderByDescending(a => a.Date)
                .ToListAsync();

            ViewBag.TeacherId = teacherId;

            return View(attendanceRecords);
        }



    }
}
