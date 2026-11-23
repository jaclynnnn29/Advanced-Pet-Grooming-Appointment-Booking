
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Advanced_Pet_Grooming.Data;
using Advanced_Pet_Grooming.Models;
using System.Security.Cryptography;

namespace Advanced_Pet_Grooming.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        private string GenerateCaptchaCode()
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";

            return new string(
                Enumerable.Range(0, 6)
                .Select(_ => chars[RandomNumberGenerator.GetInt32(chars.Length)])
                .ToArray()
            );
        }

        private void SetCaptcha()
        {
            string captcha = GenerateCaptchaCode();

            HttpContext.Session.SetString("CaptchaCode", captcha);
            ViewBag.CaptchaCode = captcha;
        }

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        // =========================
        // CUSTOMER REGISTRATION
        // =========================

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Register(Customer model, string password)
        {
            if (string.IsNullOrWhiteSpace(model.name) ||
                string.IsNullOrWhiteSpace(model.email) ||
                string.IsNullOrWhiteSpace(model.phone_no) ||
                string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error = "Please fill in all required fields.";
                return View(model);
            }

            // Check duplicate email
            string email = model.email.Trim();

            bool emailExists =
                _context.Customers.Any(c => c.email == email) ||
                _context.Staffs.Any(s => s.email == email);

            if (emailExists)
            {
                ViewBag.Error = "Email is already registered.";
                return View(model);
            }

            // Create Customer
            var customer = new Customer
            {
                name = model.name.Trim(),
                email = email,
                phone_no = model.phone_no.Trim(),
                password_hash = BCrypt.Net.BCrypt.HashPassword(password),
                account_status = "Active",
                created_at = DateTime.Now,
                failed_login_attempts = 0,
                locked_until = null
            };

            _context.Customers.Add(customer);
            _context.SaveChanges();

            TempData["Success"] =
                "Registration successful! Please log in.";

            return RedirectToAction("Login");
        }

        // =========================
        // LOGIN PAGE
        // =========================

        [HttpGet]
        public IActionResult Login()
        {
            SetCaptcha();
            return View();
        }

        // =========================
        // CUSTOMER / STAFF LOGIN
        // =========================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error = "Please enter email and password.";
                return View();
            }

            email = email.Trim();

            // =========================
            // 1. CHECK STAFF / ADMIN
            // =========================

            var staff = _context.Staffs
                .FirstOrDefault(s => s.email == email);

            if (staff != null)
            {
                if (staff.account_status != "Active")
                {
                    ViewBag.Error = "Your account is inactive.";
                    return View();
                }

                if (BCrypt.Net.BCrypt.Verify(
                    password, staff.password_hash))
                {
                    HttpContext.Session.Clear();

                    HttpContext.Session.SetInt32(
                        "UserId", staff.staff_id);

                    HttpContext.Session.SetString(
                        "UserName", staff.staff_name);

                    HttpContext.Session.SetString(
                        "UserRole", staff.role);

                    return RedirectToAction("Index", "Home");
                }

                ViewBag.Error = "Invalid email or password.";
                return View();
            }

            // =========================
            // 2. CHECK CUSTOMER
            // =========================

            var customer = _context.Customers
                .FirstOrDefault(c => c.email == email);

            if (customer == null)
            {
                ViewBag.Error = "Invalid email or password.";
                return View();
            }

            // Check account status
            if (customer.account_status != "Active")
            {
                ViewBag.Error = "Your account is inactive.";
                return View();
            }

            // Check temporary account lock
            if (customer.locked_until.HasValue &&
                customer.locked_until.Value > DateTime.Now)
            {
                ViewBag.Error =
                    "Your account is temporarily locked. Please try again later.";

                return View();
            }

            // Reset expired lock
            if (customer.locked_until.HasValue &&
                customer.locked_until.Value <= DateTime.Now)
            {
                customer.locked_until = null;
                customer.failed_login_attempts = 0;
            }

            // Verify password
            if (!BCrypt.Net.BCrypt.Verify(
                password, customer.password_hash))
            {
                customer.failed_login_attempts++;

                // Lock account after 3 failed attempts
                if (customer.failed_login_attempts >= 3)
                {
                    customer.locked_until =
                        DateTime.Now.AddMinutes(1);

                    customer.failed_login_attempts = 0;

                    _context.SaveChanges();

                    ViewBag.Error =
                        "Too many failed attempts. Your account is locked for 1 minute.";

                    return View();
                }

                _context.SaveChanges();

                ViewBag.Error =
                    $"Invalid email or password. Failed attempts: {customer.failed_login_attempts}/3";

                return View();
            }

            // =========================
            // SUCCESSFUL CUSTOMER LOGIN
            // =========================

            customer.failed_login_attempts = 0;
            customer.locked_until = null;

            _context.SaveChanges();

            HttpContext.Session.Clear();

            HttpContext.Session.SetInt32(
                "UserId", customer.customer_id);

            HttpContext.Session.SetString(
                "UserName", customer.name);

            HttpContext.Session.SetString(
                "UserRole", "Customer");

            return RedirectToAction("Index", "Home");
        }

        // =========================
        // LOGOUT
        // =========================

        [HttpGet]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();

            return RedirectToAction("Login");
        }
    }
}
