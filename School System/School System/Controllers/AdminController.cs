using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore; // <- Required for .Include
using School_System.Models;



namespace School_System.Controllers
{
    [Authorize]
    public class AdminController : Controller
    {
        private readonly SchoolManagementSystemContext context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public AdminController(SchoolManagementSystemContext _context, UserManager<IdentityUser> userManager,
    RoleManager<IdentityRole> roleManager)
        {
            context = _context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public IActionResult Index()
        {
            var totalStudents = context.Students.Count();
            var totalTeachers = context.Teachers.Count();

            var currentMonth = DateTime.Now.Month;
            var currentYear = DateTime.Now.Year;

            var totalFeesCollected = context.Fees
                .Where(f => f.PaymentStatus == "Paid"
                         && f.PaymentDate.HasValue
                         && f.PaymentDate.Value.Month == currentMonth
                         && f.PaymentDate.Value.Year == currentYear)
                .Sum(f => f.FeeAmount);

            ViewBag.TotalStudents = totalStudents;
            ViewBag.TotalTeachers = totalTeachers;
            ViewBag.TotalFeesCollected = totalFeesCollected;

            return View();
        }

        public async Task<IActionResult> ManageTeachers()
        {
            // Get all teachers
            var teachers = await context.Teachers.ToListAsync();

            // Gather all UserIds from teachers (for email lookups)
            var userIds = teachers
                .Where(t => !string.IsNullOrEmpty(t.UserId))
                .Select(t => t.UserId!)
                .ToList();

            // Map UserId -> Email from Identity
            var users = await _userManager.Users
                .Where(u => userIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => u.Email ?? "N/A");

            // Get all head teacher IDs from HeadTeacherClasses table
            var headTeacherIds = await context.HeadTeacherClasses
                .Select(h => h.TeacherId)
                .Distinct()
                .ToListAsync();

            // Build the view models
            var viewModels = teachers.Select(t => new TeacherViewModel
            {
                TeacherId = t.TeacherId,
                FullName = t.FullName ?? "",
                Qualification = t.Qualification ?? "",
                ContactInfo = t.ContactInfo ?? "",
                HireDate = t.HireDate?.ToDateTime(TimeOnly.MinValue) ?? DateTime.MinValue,
                Salary = t.Salary ?? 0,
                Email = !string.IsNullOrEmpty(t.UserId) && users.ContainsKey(t.UserId)
                    ? users[t.UserId]
                    : "N/A",
                IsHeadTeacher = headTeacherIds.Contains(t.TeacherId) // ✅ highlight flag
            }).ToList();

            return View(viewModels);
        }


        public IActionResult AssignRoles()
        {
            return View();
        }

      
        [HttpGet]
        // GET
        public IActionResult CreateTeacher()
        {
            return View();
        }

        // POST
        [HttpPost]
        [ValidateAntiForgeryToken]

        [HttpPost]
        public async Task<IActionResult> CreateTeacher(TeacherCreateViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // 1. Create IdentityUser
            var user = new IdentityUser
            {
                UserName = model.Email,
                Email = model.Email
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View(model);
            }

            // 2. Ensure "Teacher" role exists
            var roleExists = await _roleManager.RoleExistsAsync("Teacher");
            if (!roleExists)
            {
                var roleResult = await _roleManager.CreateAsync(new IdentityRole("Teacher"));
                if (!roleResult.Succeeded)
                {
                    ModelState.AddModelError("", "Failed to create Teacher role.");
                    return View(model);
                }
            }

            // 3. Assign user to "Teacher" role
            await _userManager.AddToRoleAsync(user, "Teacher");

            // 4. Create Teacher entity
            var teacher = new Teacher
            {
                FullName = model.FullName,
                Qualification = model.Qualification,
                ContactInfo = model.ContactInfo,
                HireDate = DateOnly.FromDateTime(model.HireDate),
                Salary = model.Salary,
                UserId = user.Id
            };

            context.Teachers.Add(teacher);
            await context.SaveChangesAsync();

            return RedirectToAction("ManageTeachers");
        }
     
        public async Task<IActionResult> EditTeacher(int id)
        {
            var teacher = await context.Teachers.FindAsync(id);
            if (teacher == null) return NotFound();

            var user = await _userManager.FindByIdAsync(teacher.UserId);
            if (user == null)
            {
                Console.WriteLine("DEBUG: No Identity user found for UserId = " + teacher.UserId);
            }

            var viewModel = new TeacherEditViewModel
            {
                TeacherId = teacher.TeacherId,
                FullName = teacher.FullName,
                Qualification = teacher.Qualification,
                ContactInfo = teacher.ContactInfo,
                HireDate = teacher.HireDate,
                Salary = teacher.Salary,
                Email = user?.Email ?? "",
                OldEmail = user?.Email ?? ""
            };

            return View(viewModel);
        }



        // POST: Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> EditTeacher(TeacherEditViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var teacher = await context.Teachers.FindAsync(model.TeacherId);
            if (teacher == null) return NotFound();

            // Update Identity user
            var user = await _userManager.FindByEmailAsync(model.OldEmail ?? model.Email);
            if (user != null)
            {
                user.Email = model.Email;
                user.UserName = model.Email;
                var emailResult = await _userManager.UpdateAsync(user);

                if (!emailResult.Succeeded)
                {
                    foreach (var error in emailResult.Errors)
                        ModelState.AddModelError(string.Empty, error.Description);
                    return View(model);
                }

                if (!string.IsNullOrWhiteSpace(model.Password))
                {
                    var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                    var passResult = await _userManager.ResetPasswordAsync(user, token, model.Password);

                    if (!passResult.Succeeded)
                    {
                        foreach (var error in passResult.Errors)
                            ModelState.AddModelError(string.Empty, error.Description);
                        return View(model);
                    }
                }
            }

            // Update teacher data
            teacher.FullName = model.FullName;
            teacher.Qualification = model.Qualification;
            teacher.ContactInfo = model.ContactInfo;
            teacher.HireDate = model.HireDate;
            teacher.Salary = model.Salary;

            await context.SaveChangesAsync();
            return RedirectToAction(nameof(ManageTeachers));
        }

        // GET: Delete
        [HttpPost]
        public async Task<IActionResult> DeleteTeacher(int id)
        {
            var teacher = await context.Teachers.FindAsync(id);
            if (teacher == null) return NotFound();

            if (!string.IsNullOrEmpty(teacher.UserId))
            {
                var user = await _userManager.FindByIdAsync(teacher.UserId);
                if (user != null)
                {
                    var result = await _userManager.DeleteAsync(user);
                    if (!result.Succeeded)
                    {
                        ModelState.AddModelError("", "Failed to delete user from Identity.");
                        return RedirectToAction(nameof(ManageTeachers));
                    }
                }
            }

            context.Teachers.Remove(teacher);
            await context.SaveChangesAsync();

            return RedirectToAction(nameof(ManageTeachers));
        }
        public async Task<IActionResult> TeacherProfile(int id)
        {
            var teacher = await context.Teachers
                .Include(t => t.TeacherAssignedSubjects)
                .Include(t => t.HeadTeacherClasses) // Include head teacher classes
                .FirstOrDefaultAsync(t => t.TeacherId == id);

            if (teacher == null) return NotFound();

            var user = await _userManager.FindByIdAsync(teacher.UserId);

            var unassignedSubjects = await context.Subjects
                .Include(s => s.Class)
                .Where(s => !s.IsAssigned)
                .ToListAsync();

            var model = new TeacherProfileViewModel
            {
                TeacherId = teacher.TeacherId,
                FullName = teacher.FullName ?? "",
                Email = user?.Email ?? "N/A",
                Qualification = teacher.Qualification ?? "",
                ContactInfo = teacher.ContactInfo ?? "",
                HireDate = teacher.HireDate ?? DateOnly.MinValue,
                Salary = teacher.Salary ?? 0,
                IsPaid = teacher.IsPaid,
                IsPaymentConfirmed = teacher.IsPaymentConfirmed,

                Subjects = teacher.TeacherAssignedSubjects.Select(ts => new SubjectViewModel
                {
                    Id = ts.Id,
                    SubjectId = ts.SubjectId,
                    SubjectName = ts.SubjectName,
                    ClassName = ts.ClassName
                }).ToList(),

                UnassignedSubjects = unassignedSubjects.Select(s => new SubjectViewModel
                {
                    SubjectId = s.SubjectId,
                    SubjectName = s.SubjectName ?? "",
                    ClassName = s.Class?.ClassName ?? "Unknown"
                }).ToList(),

                AllClasses = await context.Classes
                                .Select(c => new ClassViewModel
                                {
                                    ClassId = c.ClassId,
                                    ClassName = c.ClassName ?? ""
                                }).ToListAsync(),

                HeadTeacherClasses = teacher.HeadTeacherClasses
                    .Select(hc => hc.ClassName ?? "")
                    .ToList()
            };

            return View(model);
        }


        [HttpPost]
        [HttpPost]
        public async Task<IActionResult> AddSubject(int teacherId, int subjectId, string className)
        {
            var subject = await context.Subjects.FirstOrDefaultAsync(s => s.SubjectId == subjectId);

            if (subject != null && !subject.IsAssigned)
            {
                context.TeacherAssignedSubjects.Add(new TeacherAssignedSubject
                {
                    TeacherId = teacherId,
                    SubjectId = subjectId,
                    SubjectName = subject.SubjectName,
                    ClassName = className,
                    IsAssigned = true
                });

                subject.IsAssigned = true;
                await context.SaveChangesAsync();
            }

            return RedirectToAction("TeacherProfile", new { id = teacherId });
        }

        [HttpPost]
        [HttpPost]
        public async Task<IActionResult> RemoveSubject(int teacherId, int subjectId)
        {
            var assignment = await context.TeacherAssignedSubjects
                .FirstOrDefaultAsync(t => t.Id == subjectId && t.TeacherId == teacherId);

            if (assignment != null)
            {
                var subject = await context.Subjects.FindAsync(assignment.SubjectId);
                if (subject != null)
                    subject.IsAssigned = false;

                context.TeacherAssignedSubjects.Remove(assignment);
                await context.SaveChangesAsync();
            }

            return RedirectToAction("TeacherProfile", new { id = teacherId });
        }

        [HttpPost]
        [HttpPost]
        public async Task<IActionResult> RemoveAllSubjects(int teacherId)
        {
            var assignments = await context.TeacherAssignedSubjects
                .Where(t => t.TeacherId == teacherId)
                .ToListAsync();

            foreach (var assign in assignments)
            {
                var subject = await context.Subjects.FindAsync(assign.SubjectId);
                if (subject != null)
                    subject.IsAssigned = false;

                context.TeacherAssignedSubjects.Remove(assign);
            }

            await context.SaveChangesAsync();

            return RedirectToAction("TeacherProfile", new { id = teacherId });
        }


        [HttpPost]
        public IActionResult MarkPayPending(int teacherId)
        {
            var teacher = context.Teachers.Find(teacherId);
            if (teacher == null) return NotFound();

            teacher.IsPaid = true;
            context.SaveChanges();

            return RedirectToAction("TeacherProfile", new { id = teacherId });
        }
        [HttpGet]
        public async Task<IActionResult> CreateSubject()
        {
            await PopulateClassesDropdown();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateSubject([Bind("SubjectName,ClassId,IsAssigned")] Subject subject)
        {
            ModelState.Remove(nameof(Subject.Class)); // 👈 Remove validation for navigation property

            if (!ModelState.IsValid)
            {
                await PopulateClassesDropdown(subject.ClassId);
                return View(subject);
            }

            subject.IsAssigned = false;
            context.Subjects.Add(subject);
            await context.SaveChangesAsync();

            TempData["Success"] = "Subject created successfully!";
            return RedirectToAction("ManageSubjects");
        }


        private async Task PopulateClassesDropdown(int? selectedClassId = null)
        {
            ViewBag.Classes = new SelectList(await context.Classes.ToListAsync(), "ClassId", "ClassName", selectedClassId);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var subject = await context.Subjects.FindAsync(id);
            if (subject == null)
            {
                return NotFound();
            }

            context.Subjects.Remove(subject);
            await context.SaveChangesAsync();

            TempData["Success"] = "Subject deleted successfully!";
            return RedirectToAction(nameof(ManageSubjects));
        }



        public async Task<IActionResult> ManageSubjects()
        {
            var subjects = await context.Subjects
                .Include(s => s.Class)
                .Include(s => s.TeacherAssignedSubjects)
                    .ThenInclude(tas => tas.Teacher)
                .ToListAsync();

            return View(subjects);

        }
    }
}
