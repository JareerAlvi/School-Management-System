using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
  
using School_System.Models;
using SchoolManagementSystem.Models;

namespace SchoolManagementSystem.Controllers
{
    public class BoardAdmissionsController : Controller
    {
        private readonly SchoolManagementSystemContext _context;

        public BoardAdmissionsController(SchoolManagementSystemContext context)
        {
            _context = context;
        }

        // GET: BoardAdmissions
        public async Task<IActionResult> Index(string classFilter, int? yearFilter, string statusFilter)
        {
            // Step 1: Base query for Class 9 & 10 students
            var query = _context.BoardAdmissions
                .Include(b => b.Student)
                .ThenInclude(s => s.Class)
                .Where(b => b.Student.Class.ClassName == "Class 9" || b.Student.Class.ClassName == "Class 10")
                .AsQueryable();

            // Step 2: Apply Class filter
            if (!string.IsNullOrEmpty(classFilter))
            {
                query = query.Where(b => b.ClassName == classFilter);
            }

            // Step 3: Apply Year filter
            if (yearFilter.HasValue)
            {
                query = query.Where(b => b.Student.BoardYear.HasValue && b.Student.BoardYear.Value.Year == yearFilter.Value);
            }

            // Step 4: Apply Status filter
            if (!string.IsNullOrEmpty(statusFilter))
            {
                switch (statusFilter)
                {
                    case "Pending":
                        query = query.Where(b => !b.IsSent && !b.ReceivedByStudent && !b.IsRejected);
                        break;
                    case "Sent":
                        query = query.Where(b => b.IsSent && !b.IsRejected && !b.ReceivedByStudent);
                        break;
                    case "Arrived":
                        query = query.Where(b => b.IsRejected && !b.ReceivedByStudent);
                        break;
                    case "Received":
                        query = query.Where(b => b.ReceivedByStudent);
                        break;
                }
            }

            // Step 5: Populate ViewBags
            ViewBag.ClassList = await _context.Classes
                .Where(c => c.ClassName == "Class 9" || c.ClassName == "Class 10")
                .Select(c => c.ClassName)
                .Distinct()
                .ToListAsync();

            ViewBag.YearList = await _context.Students
                .Where(s => s.BoardYear.HasValue)
                .Select(s => s.BoardYear.Value.Year)
                .Distinct()
                .OrderByDescending(y => y)
                .ToListAsync();

            ViewBag.StatusList = new List<string> { "Pending", "Sent", "Arrived", "Received" };

            ViewBag.SelectedClass = classFilter;
            ViewBag.SelectedYear = yearFilter?.ToString();
            ViewBag.SelectedStatus = statusFilter;

            var boardAdmissions = await query
                .OrderBy(b => b.Student.FullName)
                .ToListAsync();

            return View(boardAdmissions);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkSent(int id)
        {
            var ba = await _context.BoardAdmissions.FindAsync(id);
            if (ba != null)
            {
                ba.IsSent = true;
                ba.IsRejected = false; // reset arrived
                ba.ReceivedByStudent = false;
                ba.SentDate = DateOnly.FromDateTime(DateTime.Now);

                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkArrived(int id)
        {
            var ba = await _context.BoardAdmissions.FindAsync(id);
            if (ba != null && ba.IsSent)
            {
                ba.IsRejected = true; // mark Arrived
                ba.ReceivedByStudent = false;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkReceived(int id)
        {
            var ba = await _context.BoardAdmissions.FindAsync(id);
            if (ba != null && ba.IsSent && ba.IsRejected) // must be Arrived first
            {
                ba.ReceivedByStudent = true;
                ba.RecievedDate = DateOnly.FromDateTime(DateTime.Now); // store received date
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
