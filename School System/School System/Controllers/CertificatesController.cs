using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using School_System.Models;
using SchoolManagementSystem.Models;

namespace SchoolManagementSystem.Controllers
{
    public class CertificatesController : Controller
    {
        private readonly SchoolManagementSystemContext _context;

        public CertificatesController(SchoolManagementSystemContext context)
        {
            _context = context;
        }


        // GET: Certificates
        // GET: Certificates
        public async Task<IActionResult> Index(int? yearFilter, string arrivalFilter, string receivingFilter)
        {
            // Ensure a Certificate exists for each Class 10 student
            var class10Students = await _context.Students
                .Include(s => s.Class)
                .Where(s => s.Class.ClassName == "Class 10")
                .ToListAsync();

            foreach (var student in class10Students)
            {
                var exists = await _context.Certificates
                    .AnyAsync(c => c.StudentId == student.StudentId);
                if (!exists)
                {
                    _context.Certificates.Add(new Certificate
                    {
                        StudentId = student.StudentId,
                        HasArrived = false,
                        HasReceived = false
                    });
                }
            }

            await _context.SaveChangesAsync();

            // Query certificates
            var query = _context.Certificates
                .Include(c => c.Student)
                .ThenInclude(s => s.Class)
                .AsQueryable();

            // Filter by BoardYear
            if (yearFilter.HasValue)
            {
                query = query.Where(c => c.Student.BoardYear.HasValue && c.Student.BoardYear.Value.Year == yearFilter.Value);
            }

            // Filter by Arrival Status
            if (!string.IsNullOrEmpty(arrivalFilter))
            {
                switch (arrivalFilter)
                {
                    case "Arrived":
                        query = query.Where(c => c.HasArrived);
                        break;
                    case "NotArrived":
                        query = query.Where(c => !c.HasArrived);
                        break;
                }
            }

            // Filter by Receiving Status
            if (!string.IsNullOrEmpty(receivingFilter))
            {
                switch (receivingFilter)
                {
                    case "Received":
                        query = query.Where(c => c.HasReceived);
                        break;
                    case "Pending":
                        query = query.Where(c => !c.HasReceived);
                        break;
                }
            }

            var certificates = await query.ToListAsync();

            // Populate dropdowns
            ViewBag.YearList = await _context.Students
                .Where(s => s.BoardYear.HasValue)
                .Select(s => s.BoardYear.Value.Year)
                .Distinct()
                .ToListAsync();

            ViewBag.SelectedYear = yearFilter;
            ViewBag.SelectedArrival = arrivalFilter;
            ViewBag.SelectedReceiving = receivingFilter;

            return View(certificates);
        }


        // POST: Mark Arrived
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkArrived(int id)
        {
            var cert = await _context.Certificates.FindAsync(id);
            if (cert != null)
            {
                cert.HasArrived = true;
                cert.arrivalDte= DateOnly.FromDateTime(DateTime.Now);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // POST: Mark Received
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkReceived(int id)
        {
            var cert = await _context.Certificates.FindAsync(id);
            if (cert != null)
            {
                cert.HasReceived = true;
                cert.receivedDate= DateOnly.FromDateTime(DateTime.Now);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
