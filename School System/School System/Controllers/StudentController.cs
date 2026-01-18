using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // <- Required for .Include
using School_System.Models;

namespace School_System.Controllers
{
    [Authorize]
    public class StudentController : Controller
    {
        private readonly SchoolManagementSystemContext context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public StudentController(SchoolManagementSystemContext _context, UserManager<IdentityUser> userManager,
    RoleManager<IdentityRole> roleManager)
        {
            context = _context;
            _userManager = userManager;
            _roleManager = roleManager;
        }
        public IActionResult ManageStudents()
        {

            var students = context.Students
                .Include(s => s.Class)
                .Include(s => s.Fees)
                .Select(s => new StudentCreateViewModel
                {
                    FullName = s.FullName,
                    DateOfBirth = s.DateOfBirth,
                    Gender = s.Gender,
                    Phone = s.Phone,
                    Email = s.Email,
                    Address = s.Address,
                    GuardianName = s.GuardianInfo,
                    AdmissionDate = s.AdmissionDate,
                    ClassName=s.Class.ClassName,
                    ClassId = s.ClassId,
                    FeeAmount = s.Fees.OrderByDescending(f => f.DueDate).FirstOrDefault() != null
                                ? s.Fees.OrderByDescending(f => f.DueDate).FirstOrDefault().FeeAmount ?? 0
                                : 0,
                    FeeDueDate = s.Fees.OrderByDescending(f => f.DueDate).FirstOrDefault() != null
                                ? s.Fees.OrderByDescending(f => f.DueDate).FirstOrDefault().DueDate
                                : null,
                    StudentId = s.StudentId
                })
                .ToList();
            ViewBag.Classes= context.Classes.ToList();
            return View(students);
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
       public IActionResult CreateStudent()
{
    ViewBag.Classes = context.Classes.ToList();
    return View(new StudentCreateViewModel());
}

[HttpPost]
        public async Task<IActionResult> CreateStudent(StudentCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Classes = context.Classes.ToList();
                return View(model);
            }

            try
            {
                if (!string.IsNullOrEmpty(model.Email))
                {
                    if (await context.Students.AnyAsync(s => s.Email == model.Email))
                    {
                        ModelState.AddModelError("Email", "This email is already registered.");
                        ViewBag.Classes = context.Classes.ToList();
                        return View(model);
                    }
                }

                var selectedClass = await context.Classes
                    .Include(c => c.Students)
                    .FirstOrDefaultAsync(c => c.ClassId == model.ClassId);

                if (selectedClass == null)
                {
                    ModelState.AddModelError("ClassId", "Selected class does not exist.");
                    ViewBag.Classes = context.Classes.ToList();
                    return View(model);
                }

                if (selectedClass.MaxCapacity.HasValue && selectedClass.Students.Count >= selectedClass.MaxCapacity)
                {
                    ModelState.AddModelError("ClassId", "Selected class is full.");
                    ViewBag.Classes = context.Classes.ToList();
                    return View(model);
                }

                var student = new Student
                {
                    FullName = model.FullName,
                    DateOfBirth = model.DateOfBirth,
                    Gender = model.Gender,
                    Phone = model.Phone,
                    Email = model.Email,
                    Address = model.Address,
                    GuardianInfo = model.GuardianName,
                    AdmissionDate = model.AdmissionDate,
                    ClassId = model.ClassId
                };

                context.Students.Add(student);
                await context.SaveChangesAsync();

                // Fee due date = 10th of next month
                var now = DateTime.Now;
                int nextMonth = now.Month == 12 ? 1 : now.Month + 1;
                int year = now.Month == 12 ? now.Year + 1 : now.Year;
                var dueDate = new DateOnly(year, nextMonth, 10);

                var fee = new Fee
                {
                    StudentId = student.StudentId,
                    FeeAmount = model.FeeAmount,
                    PaymentStatus = "Pending",
                    DueDate = dueDate
                };

                context.Fees.Add(fee);
                await context.SaveChangesAsync();

                return RedirectToAction("ManageStudents");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"An error occurred: {ex.Message}");
                ViewBag.Classes = context.Classes.ToList();
                return View(model);
            }
        }

        public async Task<IActionResult> DeleteStudent(int id)
        {
            var student = await context.Students
                .Include(s => s.Fees)
                .FirstOrDefaultAsync(s => s.StudentId == id);

            if (student == null) return NotFound();

            // Delete related fees first
            if (student.Fees.Any())
            {
                context.Fees.RemoveRange(student.Fees);
            }

            // Delete student after fees
            context.Students.Remove(student);
            await context.SaveChangesAsync();

            return RedirectToAction(nameof(ManageStudents));
        }

        // GET: EditStudent
        public async Task<IActionResult> EditStudent(int id)
        {
            var student = await context.Students.FindAsync(id);
            if (student == null) return NotFound();

            var fee = await context.Fees.FirstOrDefaultAsync(f => f.StudentId == id);

            ViewBag.Classes = context.Classes.ToList();

            var viewModel = new StudentCreateViewModel
            {
                StudentId = student.StudentId,
                FullName = student.FullName,
                DateOfBirth = student.DateOfBirth,
                Gender = student.Gender,
                Phone = student.Phone,
                Email = student.Email,
                Address = student.Address,
                GuardianName = student.GuardianInfo,
                AdmissionDate = student.AdmissionDate,
                ClassId = student.ClassId,
                FeeAmount = fee?.FeeAmount ?? 0,
                FeeDueDate = fee?.DueDate
            };

            return View(viewModel);
        }

        // POST: EditStudent
        [HttpPost]
        public async Task<IActionResult> EditStudent(StudentCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Classes = context.Classes.ToList();
                return View(model);
            }

            try
            {
                var student = await context.Students
                    .Include(s => s.Fees)
                    .FirstOrDefaultAsync(s => s.StudentId == model.StudentId);

                if (student == null) return NotFound();

                // Check for duplicate email if changed
                if (!string.IsNullOrEmpty(model.Email))
                {
                    var emailExists = await context.Students
                        .AnyAsync(s => s.Email == model.Email && s.StudentId != model.StudentId);

                    if (emailExists)
                    {
                        ModelState.AddModelError("Email", "This email is already registered.");
                        ViewBag.Classes = context.Classes.ToList();
                        return View(model);
                    }
                }

                // Check class capacity if changed
                if (student.ClassId != model.ClassId)
                {
                    var selectedClass = await context.Classes
                        .Include(c => c.Students)
                        .FirstOrDefaultAsync(c => c.ClassId == model.ClassId);

                    if (selectedClass == null)
                    {
                        ModelState.AddModelError("ClassId", "Selected class does not exist.");
                        ViewBag.Classes = context.Classes.ToList();
                        return View(model);
                    }

                    if (selectedClass.MaxCapacity.HasValue &&
                        selectedClass.Students.Count >= selectedClass.MaxCapacity)
                    {
                        ModelState.AddModelError("ClassId", "Selected class is already full.");
                        ViewBag.Classes = context.Classes.ToList();
                        return View(model);
                    }
                }

                // Update student fields
                student.FullName = model.FullName;
                student.DateOfBirth = model.DateOfBirth;
                student.Gender = model.Gender;
                student.Phone = model.Phone;
                student.Email = model.Email;
                student.Address = model.Address;
                student.GuardianInfo = model.GuardianName;
                student.AdmissionDate = model.AdmissionDate;
                student.ClassId = model.ClassId;

                // Update Fee (if exists) or add new
                var fee = await context.Fees.FirstOrDefaultAsync(f => f.StudentId == student.StudentId);
                if (fee != null)
                {
                    fee.FeeAmount = model.FeeAmount;
                    fee.DueDate = model.FeeDueDate;
                }
                else
                {
                    context.Fees.Add(new Fee
                    {
                        StudentId = student.StudentId,
                        FeeAmount = model.FeeAmount,
                        DueDate = model.FeeDueDate,
                        PaymentStatus = model.PaymentStatus
                    });
                }

                await context.SaveChangesAsync();
                return RedirectToAction(nameof(ManageStudents));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"An error occurred while updating the student. {ex.Message}");
                ViewBag.Classes = context.Classes.ToList();
                return View(model);
            }
        }
        public async Task<IActionResult> StudentProfile(int id)
        {
            var student = await context.Students
                .Include(s => s.Class)
                .FirstOrDefaultAsync(s => s.StudentId == id);

            if (student == null) return NotFound();

         

            var fees = await context.Fees
                .Where(f => f.StudentId == id)
                .OrderByDescending(f => f.DueDate)
                .ToListAsync();

            var latestFee = fees.FirstOrDefault();

            var viewModel = new StudentProfileViewModel
            {
                Student = student,
                StudentId = student.StudentId,
                FullName = student.FullName,
                DateOfBirth = student.DateOfBirth,
                AdmissionDate = student.AdmissionDate,
                Gender = student.Gender,
                Phone = student.Phone,
                Email = student.Email,
                Address = student.Address,
                GuardianName = student.GuardianInfo,
                ClassName = student.Class?.ClassName,
                Fees = fees,

                LastFeeAmount = latestFee?.FeeAmount,
                LastFeeDueDate = latestFee?.DueDate,
                LastFeeStatus = latestFee?.PaymentStatus
            };

            return View(viewModel);
        }
        public async Task<IActionResult> GenerateMonthlyFees(string studentName = null, int? dueMonth = null, string paymentStatus = null)
        {
            var today = DateTime.Today;
            bool feeRecordExists = await context.FeeGenerationRecord
                .AnyAsync(fgr => fgr.Year == today.Year && fgr.Month == today.Month);
            bool enableButton = today.Day >= 1 && !feeRecordExists;

            // Base query including student
            var query = context.Fees.Include(f => f.Student).AsQueryable();

            // Apply filters if any
            if (!string.IsNullOrEmpty(studentName))
            {
                query = query.Where(f => f.Student.FullName.Contains(studentName));
            }

            if (dueMonth.HasValue && dueMonth.Value >= 1 && dueMonth.Value <= 12)
            {
                query = query.Where(f => f.DueDate.HasValue && f.DueDate.Value.Month == dueMonth.Value);

            }

            if (!string.IsNullOrEmpty(paymentStatus))
            {
                if (paymentStatus == "Paid")
                    query = query.Where(f => f.PaymentStatus == "Paid");
                else if (paymentStatus == "Unpaid")
                    query = query.Where(f => f.PaymentStatus != "Paid");
            }

            var filteredFees = await query.OrderByDescending(f => f.DueDate).ToListAsync();

            // For dropdowns, get distinct values
            var studentNames = await context.Students.Select(s => s.FullName).Distinct().OrderBy(n => n).ToListAsync();
            var months = Enumerable.Range(1, 12).ToList(); // Jan to Dec
            var paymentStatuses = new List<string> { "Paid", "Unpaid" };

            var model = new GenerateMonthlyFeesViewModel
            {
                Fees = filteredFees,
                FilterStudentName = studentName,
                FilterDueMonth = dueMonth,
                FilterPaymentStatus = paymentStatus,
                AvailableStudentNames = studentNames,
                AvailableMonths = months,
                AvailablePaymentStatuses = paymentStatuses
            };

            ViewBag.EnableGenerateButton = enableButton;
            ViewBag.Check = feeRecordExists;

            return View(model);
        }




        [HttpPost]
        public async Task<IActionResult> GenerateMonthlyFeesPost()
        {
            var today = DateTime.Today;

            bool feeRecordExists = await context.FeeGenerationRecord
                .AnyAsync(fgr => fgr.Year == today.Year && fgr.Month == today.Month);

            if (feeRecordExists)
            {
                TempData["Message"] = "Fees already generated for this month.";
                return RedirectToAction(nameof(GenerateMonthlyFees));
            }

            var students = await context.Students.Include(s => s.Fees).ToListAsync();

            var dueDate = new DateOnly(today.Year, today.Month, 10);

            foreach (var student in students)
            {
                // Check if fee already exists for this student for current month/year
                bool feeExistsForStudent = student.Fees.Any(f => f.DueDate.HasValue &&
                                                                 f.DueDate.Value.Year == today.Year &&
                                                                 f.DueDate.Value.Month == today.Month);

                if (feeExistsForStudent)
                {
                    // Skip fee generation for this student to avoid duplicate
                    continue;
                }

                var lastFeeAmount = student.Fees
                    .OrderByDescending(f => f.DueDate)
                    .FirstOrDefault()?.FeeAmount ?? 0m;

                var fee = new Fee
                {
                    StudentId = student.StudentId,
                    FeeAmount = lastFeeAmount,
                    PaymentStatus = "Unpaid",
                    DueDate = dueDate
                };

                context.Fees.Add(fee);
            }

            context.FeeGenerationRecord.Add(new FeeGenerationRecord
            {
                Year = today.Year,
                Month = today.Month,
                GeneratedOn = DateTime.Now
            });

            await context.SaveChangesAsync();

            TempData["Message"] = "Monthly fees generated successfully.";

            return RedirectToAction(nameof(GenerateMonthlyFees));
        }





        [HttpPost]
        public async Task<IActionResult> ReceiveFee(int studentId)
        {
            var fee = await context.Fees
                .Where(f => f.StudentId == studentId &&
                            (f.PaymentStatus == "Pending" || f.PaymentStatus == "Unpaid"))
                .OrderByDescending(f => f.DueDate) // Ensure the most recent is selected
                .FirstOrDefaultAsync();

            if (fee == null)
            {
                TempData["Error"] = "No unpaid or pending fee found.";
                return RedirectToAction(nameof(StudentProfile), new { id = studentId });
            }

            fee.PaymentStatus = "Paid";
            fee.PaymentDate = DateOnly.FromDateTime(DateTime.Now);
            fee.PaymentMethod = "Cash"; // optional

            await context.SaveChangesAsync();

            TempData["Success"] = "Fee marked as received.";
            return RedirectToAction(nameof(StudentProfile), new { id = studentId });
        }


        [HttpPost]
        public async Task<IActionResult> ReceiveFeeBulk(int feeId)
        {
            var fee = await context.Fees
                .Include(f => f.Student) // to get StudentId for redirect
                .FirstOrDefaultAsync(f => f.FeeId == feeId);

            if (fee == null)
            {
                TempData["Error"] = "Fee record not found.";
                return RedirectToAction(nameof(GenerateMonthlyFees)); // or wherever
            }

            if (fee.PaymentStatus == "Paid")
            {
                TempData["Error"] = "Fee is already marked as paid.";
                return RedirectToAction(nameof(GenerateMonthlyFees));
            }

            fee.PaymentStatus = "Paid";
            fee.PaymentDate = DateOnly.FromDateTime(DateTime.Now);
            fee.PaymentMethod = "Cash"; // optional

            await context.SaveChangesAsync();

            TempData["Success"] = $"Fee for student {fee.Student.FullName} marked as paid.";

            // Redirect to GenerateMonthlyFees or to StudentProfile of this fee's student:
            return RedirectToAction(nameof(GenerateMonthlyFees));
            // OR if you want:
            // return RedirectToAction("StudentProfile", "Student", new { id = fee.StudentId });
        }


    }
}
