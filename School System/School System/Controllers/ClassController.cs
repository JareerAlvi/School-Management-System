using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using School_System.Models;

namespace School_System.Controllers
{
    public class ClassController : Controller
    {
        private readonly SchoolManagementSystemContext _context;

        public ClassController(SchoolManagementSystemContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var classes = await _context.Classes
     .Include(c => c.Students)
     .ToListAsync();
            
            return View(classes);

        }

        public async Task<IActionResult> Create()
        {
            // Get list of teachers for dropdown (Id and FullName)
            var teachers = await _context.Teachers
                                .OrderBy(t => t.FullName)
                                .Select(t => new { t.TeacherId, t.FullName })
                                .ToListAsync();

            ViewBag.Teachers = teachers;
            return View();
        }

        // POST Create action
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Class @class)
        {
            if (ModelState.IsValid)
            {
                // Add class first
                _context.Classes.Add(@class);
                await _context.SaveChangesAsync();

                if (@class.HeadTeacherID.HasValue)
                {
                    // Add a new HeadTeacherClasses record
                    var exists = await _context.HeadTeacherClasses
                        .AnyAsync(htc => htc.TeacherId == @class.HeadTeacherID.Value && htc.ClassName == @class.ClassName);

                    if (!exists)
                    {
                        var htc = new HeadTeacherClasses
                        {
                            TeacherId = @class.HeadTeacherID.Value,
                            ClassName = @class.ClassName
                        };
                        _context.HeadTeacherClasses.Add(htc);
                        await _context.SaveChangesAsync();
                    }
                }

                return RedirectToAction(nameof(Index));
            }

            // Repopulate teachers if model state invalid
            ViewBag.Teachers = await _context.Teachers
                                    .OrderBy(t => t.FullName)
                                    .Select(t => new { t.TeacherId, t.FullName })
                                    .ToListAsync();

            return View(@class);
        }
        // GET: Class/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var cls = await _context.Classes.FindAsync(id);
            if (cls == null)
                return NotFound();

            // Load Teachers for dropdown
            ViewBag.Teachers = await _context.Teachers
                .OrderBy(t => t.FullName)
                .ToListAsync();

            return View(cls);
        }

        // POST Edit action
        // POST Edit action
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Class cls)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Update class
                    _context.Update(cls);
                    await _context.SaveChangesAsync();

                    if (cls.HeadTeacherID.HasValue)
                    {
                        // Find the existing HeadTeacherClasses record for this teacher and old class name (if any)
                        var existingHTC = await _context.HeadTeacherClasses
                            .FirstOrDefaultAsync(htc => htc.TeacherId == cls.HeadTeacherID.Value && htc.ClassName == cls.ClassName);

                        if (existingHTC != null)
                        {
                            // Update ClassName if changed (assuming cls.ClassName might have changed)
                            existingHTC.ClassName = cls.ClassName;
                            _context.HeadTeacherClasses.Update(existingHTC);
                        }
                        else
                        {
                            // Or add if missing
                            var htc = new HeadTeacherClasses
                            {
                                TeacherId = cls.HeadTeacherID.Value,
                                ClassName = cls.ClassName
                            };
                            _context.HeadTeacherClasses.Add(htc);
                        }

                        await _context.SaveChangesAsync();
                    }

                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ClassExists(cls.ClassId))
                        return NotFound();
                    else
                        throw;
                }
            }

            // Repopulate teachers on validation failure
            ViewBag.Teachers = await _context.Teachers
                                    .OrderBy(t => t.FullName)
                                    .Select(t => new { t.TeacherId, t.FullName })
                                    .ToListAsync();

            return View(cls);
        }

        private bool ClassExists(int id)
        {
            return _context.Classes.Any(e => e.ClassId == id);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var cls = await _context.Classes
                .Include(c => c.Students)
                .FirstOrDefaultAsync(c => c.ClassId == id);

            if (cls == null)
            {
                TempData["ErrorMessage"] = "Class not found.";
                return RedirectToAction(nameof(Index));
            }

            // Check for students
            if (cls.Students != null && cls.Students.Any())
            {
                TempData["ErrorMessage"] = $"Cannot delete class '{cls.ClassName}' because it still has {cls.Students.Count} enrolled student(s).";
                return RedirectToAction(nameof(Index));
            }

            // Remove any HeadTeacherClasses linked to this class
            var headTeacherLinks = await _context.HeadTeacherClasses
                .Where(htc => htc.ClassName == cls.ClassName)
                .ToListAsync();

            if (headTeacherLinks.Any())
            {
                _context.HeadTeacherClasses.RemoveRange(headTeacherLinks);
            }

            // Remove the class itself
            _context.Classes.Remove(cls);

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Class '{cls.ClassName}' deleted successfully.";

            return RedirectToAction(nameof(Index));
        }


    }
}
