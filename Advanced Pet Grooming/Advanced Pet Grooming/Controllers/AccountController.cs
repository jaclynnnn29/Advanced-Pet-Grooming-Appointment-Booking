
using Advanced_Pet_Grooming.Data;
using Advanced_Pet_Grooming.Models;
using Advanced_Pet_Grooming.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;

namespace Advanced_Pet_Grooming.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly EmailService _emailService;
        public AccountController(ApplicationDbContext context, EmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        // =========================
        // CAPTCHA
        // =========================

        private string GenerateCaptchaCode()
        {
            string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
            string captcha = "";

            for (int i = 0; i < 6; i++)
            {
                int index = RandomNumberGenerator.GetInt32(chars.Length);
                captcha += chars[index];
            }

            return captcha;
        }

        private void SetCaptcha()
        {
            string captcha = GenerateCaptchaCode();

            HttpContext.Session.SetString("CaptchaCode", captcha);
            ViewBag.CaptchaCode = captcha;
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
            // Check required fields
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

            bool emailExists = false;

            if (_context.Customers.Any(c => c.email == email))
            {
                emailExists = true;
            }

            if (_context.Staffs.Any(s => s.email == email))
            {
                emailExists = true;
            }

            if (emailExists)
            {
                ViewBag.Error = "Email is already registered.";
                return View(model);
            }

            // Validate phone number
            bool phoneValid = true;

            if (model.phone_no.Length < 10 || model.phone_no.Length > 11)
            {
                phoneValid = false;
            }

            for (int i = 0; i < model.phone_no.Length; i++)
            {
                if (model.phone_no[i] < '0' || model.phone_no[i] > '9')
                {
                    phoneValid = false;
                }
            }

            if (!phoneValid)
            {
                ViewBag.Error = "Phone number must contain 10 to 11 digits only.";
                return View(model);
            }

            // Create new customer
            Customer customer = new Customer();

            customer.name = model.name.Trim();
            customer.email = email;
            customer.phone_no = model.phone_no.Trim();

            // Hash password before saving
            customer.password_hash =
                BCrypt.Net.BCrypt.HashPassword(password);

            customer.account_status = "Active";
            customer.created_at = DateTime.Now;
            customer.failed_login_attempts = 0;
            customer.locked_until = null;

            _context.Customers.Add(customer);
            _context.SaveChanges();

            TempData["Success"] = "Registration successful! Please log in.";

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
        // FORGOT PASSWORD PAGE
        // =========================

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                ViewBag.Error = "Please enter your email.";
                return View();
            }

            email = email.Trim();

            // Find customer account
            Customer? customer = _context.Customers
                .FirstOrDefault(c => c.email == email);

            if (customer == null)
            {
                ViewBag.Error = "Email address was not found.";
                return View();
            }

            // Generate reset token
            string resetToken = Convert.ToHexString(
                RandomNumberGenerator.GetBytes(32));

            // Save reset token
            customer.reset_token = resetToken;

            // Token expires after 15 minutes
            customer.reset_token_expiry = DateTime.Now.AddMinutes(15);

            _context.SaveChanges();

            // Create reset password link for testing
            string? resetLink = Url.Action(
                "ResetPassword",
                "Account",
                new { token = resetToken },
                Request.Scheme
            );

            if (string.IsNullOrEmpty(resetLink))
            {
                ViewBag.Error = "Unable to generate reset link.";
                return View();
            }
            string emailBody = $@"
                <h2>Pet Grooming Appointment System</h2>
                <p>You requested to reset your password.</p>
                <p>Click the link below to reset your password:</p>
                <a href='{resetLink}'>Reset Password</a>
                <p>This link will expire in 15 minutes.</p>
            ";

            await _emailService.SendEmailAsync(
                email,
                "Pet Grooming System - Password Reset",
                emailBody
            );

            ViewBag.Message = "Password reset link has been sent to your email.";

            return View();
        }

        // =========================
        // RESET PASSWORD PAGE
        // =========================

        [HttpGet]
        public IActionResult ResetPassword(string token)
        {
            // Find customer using reset token
            Customer? customer = _context.Customers
                .FirstOrDefault(c => c.reset_token == token);

            if (customer == null)
            {
                return Content("Invalid password reset link.");
            }

            // Check token expiry
            if (customer.reset_token_expiry == null ||
                customer.reset_token_expiry < DateTime.Now)
            {
                return Content("This password reset link has expired.");
            }

            ViewBag.Token = token;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ResetPassword(
            string token,
            string newPassword,
            string confirmPassword)
        {
            Customer? customer = _context.Customers
                .FirstOrDefault(c => c.reset_token == token);

            if (customer == null)
            {
                ViewBag.Error = "Invalid reset link.";
                ViewBag.Token = token;
                return View();
            }

            if (customer.reset_token_expiry == null ||
                customer.reset_token_expiry < DateTime.Now)
            {
                ViewBag.Error = "Reset link has expired.";
                ViewBag.Token = token;
                return View();
            }

            if (string.IsNullOrWhiteSpace(newPassword) ||
                string.IsNullOrWhiteSpace(confirmPassword))
            {
                ViewBag.Error = "Please fill in both password fields.";
                ViewBag.Token = token;
                return View();
            }

            if (newPassword != confirmPassword)
            {
                ViewBag.Error = "Passwords do not match.";
                ViewBag.Token = token;
                return View();
            }

            customer.password_hash =
                BCrypt.Net.BCrypt.HashPassword(newPassword);

            customer.reset_token = null;
            customer.reset_token_expiry = null;

            _context.SaveChanges();

            TempData["Success"] = "Password reset successful! Please log in.";

            return RedirectToAction("Login");
        }

        // =========================
        // CUSTOMER / STAFF LOGIN
        // =========================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(
            string email,
            string password,
            string captchaAnswer)
        {
            // Get CAPTCHA from Session
            string? correctCaptcha =
                HttpContext.Session.GetString("CaptchaCode");

            // Remove old CAPTCHA so it cannot be reused
            HttpContext.Session.Remove("CaptchaCode");

            // Check CAPTCHA
            if (string.IsNullOrWhiteSpace(captchaAnswer) ||
                string.IsNullOrEmpty(correctCaptcha) ||
                captchaAnswer.Trim().ToUpper() != correctCaptcha)
            {
                ViewBag.Error = "Incorrect CAPTCHA. Please try again.";

                SetCaptcha();
                return View();
            }

            // Check email and password
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error = "Please enter your correct email and password.";

                SetCaptcha();
                return View();
            }

            email = email.Trim();

            // =========================
            // 1. STAFF / ADMIN LOGIN
            // =========================

            Staff? staff = _context.Staffs.FirstOrDefault(s => s.email == email);

            if (staff != null)
            {
                if (staff.account_status != "Active")
                {
                    ViewBag.Error = "Your account is inactive.";

                    SetCaptcha();
                    return View();
                }

                // Check staff password
                bool passwordCorrect =
                    BCrypt.Net.BCrypt.Verify(password, staff.password_hash);

                if (passwordCorrect)
                {
                    HttpContext.Session.Clear();

                    HttpContext.Session.SetInt32("UserId", staff.staff_id);

                    HttpContext.Session.SetString("UserName", staff.staff_name);

                    HttpContext.Session.SetString("UserRole", staff.role);

                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ViewBag.Error = "Invalid email or password.";

                    SetCaptcha();
                    return View();
                }
            }

            // =========================
            // 2. CUSTOMER LOGIN
            // =========================

            Customer? customer = _context.Customers.FirstOrDefault(c => c.email == email);

            if (customer == null)
            {
                ViewBag.Error = "Invalid email or password.";

                SetCaptcha();
                return View();
            }

            // Check account status
            if (customer.account_status != "Active")
            {
                ViewBag.Error = "Your account is inactive.";

                SetCaptcha();
                return View();
            }

            // Check whether account is locked
            if (customer.locked_until.HasValue && customer.locked_until.Value > DateTime.Now)
            {
                ViewBag.Error ="Your account is temporarily locked. Please try again later.";

                SetCaptcha();
                return View();
            }

            // Reset expired lock
            if (customer.locked_until.HasValue && customer.locked_until.Value <= DateTime.Now)
            {
                customer.locked_until = null;
                customer.failed_login_attempts = 0;
            }

            // Check customer password
            bool customerPasswordCorrect =
                BCrypt.Net.BCrypt.Verify(
                    password, customer.password_hash);

            if (!customerPasswordCorrect)
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

                    SetCaptcha();
                    return View();
                }

                _context.SaveChanges();

                ViewBag.Error ="Invalid email or password. Failed attempts: " + customer.failed_login_attempts + "/3";

                SetCaptcha();
                return View();
            }

            // =========================
            // SUCCESSFUL CUSTOMER LOGIN
            // =========================

            customer.failed_login_attempts = 0;
            customer.locked_until = null;

            _context.SaveChanges();

            HttpContext.Session.Clear();

            HttpContext.Session.SetInt32("UserId", customer.customer_id);

            HttpContext.Session.SetString("UserName", customer.name);

            HttpContext.Session.SetString("UserRole", "Customer");

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
