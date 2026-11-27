using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Advanced_Pet_Grooming.Data;
using Advanced_Pet_Grooming.Models;

namespace Advanced_Pet_Grooming.Controllers
{
    public class AdminStaffController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminStaffController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Helper: Verify Admin session (matches "UserRole" in _Layout.cshtml & AccountController)
        private bool IsAdmin()
        {
            return HttpContext.Session.GetString("UserRole") == "Admin";
        }

        // 4.2 READ: View Staff & Groomer Profile Directory
        public async Task<IActionResult> Index(string? search, string? roleFilter)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            var query = _context.Staffs.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(s => s.staff_name.Contains(search) || s.email.Contains(search));
            }

            if (!string.IsNullOrWhiteSpace(roleFilter))
            {
                query = query.Where(s => s.role == roleFilter);
            }

            ViewBag.Search = search;
            ViewBag.RoleFilter = roleFilter;
            return View(await query.OrderBy(s => s.staff_id).ToListAsync());
        }

        // 4.1 CREATE: Add New Staff / Groomer (GET)
        public IActionResult Create()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");
            return View();
        }

        // 4.1 CREATE: Add New Staff / Groomer (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Staff model, string plainPassword)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            // Remove password_hash from ModelState validation since we hash plainPassword manually
            ModelState.Remove("password_hash");

            // Check duplicate email (Activity Diagram 4.1)
            bool emailExists = await _context.Staffs.AnyAsync(s => s.email == model.email);
            if (emailExists)
            {
                ModelState.AddModelError("email", "This email is already registered to another staff account.");
            }

            if (string.IsNullOrWhiteSpace(plainPassword) || plainPassword.Length < 6)
            {
                ModelState.AddModelError("plainPassword", "Password must be at least 6 characters long.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            model.password_hash = BCrypt.Net.BCrypt.HashPassword(plainPassword);
            model.account_status = "Active";

            _context.Staffs.Add(model);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Staff profile for {model.staff_name} created successfully!";
            return RedirectToAction(nameof(Index));
        }

        // 4.3 UPDATE: Edit Staff / Groomer Profile (GET)
        public async Task<IActionResult> Edit(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            var staff = await _context.Staffs.FindAsync(id);
            if (staff == null) return NotFound();
            return View(staff);
        }

        // 4.3 UPDATE: Edit Staff / Groomer Profile (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Staff updatedStaff, string? newPassword)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            if (id != updatedStaff.staff_id) return BadRequest();

            ModelState.Remove("password_hash");

            // Ensure edited email doesn't collide with another staff member
            bool emailTaken = await _context.Staffs
                .AnyAsync(s => s.email == updatedStaff.email && s.staff_id != id);
            if (emailTaken)
            {
                ModelState.AddModelError("email", "Another staff member is already using this email.");
            }

            if (!ModelState.IsValid)
            {
                return View(updatedStaff);
            }

            var existingStaff = await _context.Staffs.FindAsync(id);
            if (existingStaff == null) return NotFound();

            existingStaff.staff_name = updatedStaff.staff_name;
            existingStaff.role = updatedStaff.role;
            existingStaff.email = updatedStaff.email;
            existingStaff.phone_no = updatedStaff.phone_no;
            existingStaff.account_status = updatedStaff.account_status;

            // Optional password reset by Admin
            if (!string.IsNullOrWhiteSpace(newPassword))
            {
                existingStaff.password_hash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Updated profile for {existingStaff.staff_name}.";
            return RedirectToAction(nameof(Index));
        }

        // 4.4 DELETE / DEACTIVATE: Remove / Deactivate Groomer Profile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Deactivate(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            var staff = await _context.Staffs.FindAsync(id);
            if (staff == null) return NotFound();

            // Check Active Appointments for Groomer/Staff (Activity Diagram 4.4)
            bool hasActiveAppointments = await _context.Appointments
                .AnyAsync(a => a.staff_id == id &&
                              (a.appointment_status == "Pending" || a.appointment_status == "Confirmed"));

            if (hasActiveAppointments)
            {
                TempData["ErrorMessage"] = $"Unable to deactivate {staff.staff_name}: They still have Pending or Confirmed appointments assigned.";
                return RedirectToAction(nameof(Index));
            }

            staff.account_status = "Inactive";
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"{staff.staff_name} has been deactivated.";
            return RedirectToAction(nameof(Index));
        }
    }
}