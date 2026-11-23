using Microsoft.AspNetCore.Mvc;
using Advanced_Pet_Grooming.Data;
using Advanced_Pet_Grooming.Models;

namespace Advanced_Pet_Grooming.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Account/Register
        public IActionResult Register() => View();

        // POST: /Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Register(Customer model, string password)
        {
            if (_context.Customers.Any(c => c.email == model.email))
            {
                ViewBag.Error = "Email is already registered.";
                return View(model);
            }

            model.password_hash = BCrypt.Net.BCrypt.HashPassword(password);
            model.account_status = "Active";
            model.created_at = DateTime.Now;

            _context.Customers.Add(model);
            _context.SaveChanges();

            TempData["Success"] = "Registration successful! Please log in.";
            return RedirectToAction("Login");
        }

        // GET: /Account/Login
        public IActionResult Login() => View();

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(string email, string password)
        {
            // 1. Check Staff / Admin / Groomer table first
            var staff = _context.Staffs.FirstOrDefault(s => s.email == email && s.account_status == "Active");
            if (staff != null && BCrypt.Net.BCrypt.Verify(password, staff.password_hash))
            {
                HttpContext.Session.SetInt32("UserId", staff.staff_id);
                HttpContext.Session.SetString("UserName", staff.staff_name);
                HttpContext.Session.SetString("UserRole", staff.role); // Admin, Groomer, Staff
                return RedirectToAction("Index", "Home");
            }

            // 2. Check Customer table
            var customer = _context.Customers.FirstOrDefault(c => c.email == email && c.account_status == "Active");
            if (customer != null && BCrypt.Net.BCrypt.Verify(password, customer.password_hash))
            {
                HttpContext.Session.SetInt32("UserId", customer.customer_id);
                HttpContext.Session.SetString("UserName", customer.name);
                HttpContext.Session.SetString("UserRole", "Customer");
                return RedirectToAction("Index", "Home");
            }

            ViewBag.Error = "Invalid email or password.";
            return View();
        }

        // GET: /Account/Logout
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}