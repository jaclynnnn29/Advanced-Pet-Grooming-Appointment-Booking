
using Advanced_Pet_Grooming.Data;
using Advanced_Pet_Grooming.Models;
using Microsoft.AspNetCore.Mvc;

namespace Advanced_Pet_Grooming.Controllers
{
    public class CustomerProfileController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CustomerProfileController(ApplicationDbContext context)
        {
            _context = context;
        }

        // View Customer Profile
        public IActionResult Index()
        {
            // Get logged-in Customer ID
            int? customerId = HttpContext.Session.GetInt32("UserId");

            if (customerId == null ||
                HttpContext.Session.GetString("UserRole") != "Customer")
            {
                return RedirectToAction("Login", "Account");
            }

            // Retrieve current Customer profile
            var customer = _context.Customers
                .FirstOrDefault(c => c.customer_id == customerId);

            if (customer == null || customer.account_status != "Active")
            {
                return RedirectToAction("Login", "Account");
            }

            return View(customer);
        }


        // GET: Edit Customer Profile
        [HttpGet]
        public IActionResult Edit()
        {
            int? customerId = HttpContext.Session.GetInt32("UserId");

            if (customerId == null ||
                HttpContext.Session.GetString("UserRole") != "Customer")
            {
                return RedirectToAction("Login", "Account");
            }

            var customer = _context.Customers
                .FirstOrDefault(c => c.customer_id == customerId);

            if (customer == null || customer.account_status != "Active")
            {
                return RedirectToAction("Login", "Account");
            }

            return View(customer);
        }

        // POST: Save Customer Profile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(string name, string email, string phone_no)
        {
            int? customerId = HttpContext.Session.GetInt32("UserId");

            if (customerId == null ||
                HttpContext.Session.GetString("UserRole") != "Customer")
            {
                return RedirectToAction("Login", "Account");
            }

            var customer = _context.Customers
                .FirstOrDefault(c => c.customer_id == customerId);

            if (customer == null || customer.account_status != "Active")
            {
                return RedirectToAction("Login", "Account");
            }

            // Keep entered values when validation fails
            customer.name = name?.Trim() ?? "";
            customer.email = email?.Trim() ?? "";
            customer.phone_no = phone_no?.Trim() ?? "";

            // Check required fields
            if (string.IsNullOrWhiteSpace(customer.name) ||
                string.IsNullOrWhiteSpace(customer.email) ||
                string.IsNullOrWhiteSpace(customer.phone_no))
            {
                ViewBag.Error = "All fields are required.";
                return View(customer);
            }

            // Check email format
            if (!new System.ComponentModel.DataAnnotations.EmailAddressAttribute()
                .IsValid(customer.email))
            {
                ViewBag.Error = "Please enter a valid email address.";
                return View(customer);
            }

            // Check phone number
            if (customer.phone_no.Length < 10 ||
                customer.phone_no.Length > 11 ||
                !customer.phone_no.All(char.IsDigit))
            {
                ViewBag.Error = "Phone number must contain 10 to 11 digits only.";
                return View(customer);
            }

            // Check duplicate Customer email
            bool customerEmailExists = _context.Customers.Any(c =>
                c.customer_id != customerId &&
                c.email.ToLower() == customer.email.ToLower());

            // Check duplicate Staff email
            bool staffEmailExists = _context.Staffs.Any(s =>
                s.email.ToLower() == customer.email.ToLower());

            if (customerEmailExists || staffEmailExists)
            {
                ViewBag.Error = "This email address is already registered.";
                return View(customer);
            }

            // Save updated information
            _context.SaveChanges();

            // Update Session username
            HttpContext.Session.SetString("UserName", customer.name);

            return RedirectToAction("Index");
        }


        // GET: Confirm Delete Account
        [HttpGet]
        public IActionResult Delete()
        {
            int? customerId = HttpContext.Session.GetInt32("UserId");

            if (customerId == null ||
                HttpContext.Session.GetString("UserRole") != "Customer")
            {
                return RedirectToAction("Login", "Account");
            }

            var customer = _context.Customers
                .FirstOrDefault(c => c.customer_id == customerId);

            if (customer == null || customer.account_status != "Active")
            {
                return RedirectToAction("Login", "Account");
            }

            return View(customer);
        }

        // POST: Delete Customer Account
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed()
        {
            int? customerId = HttpContext.Session.GetInt32("UserId");

            if (customerId == null ||
                HttpContext.Session.GetString("UserRole") != "Customer")
            {
                return RedirectToAction("Login", "Account");
            }

            var customer = _context.Customers
                .FirstOrDefault(c => c.customer_id == customerId);

            if (customer == null || customer.account_status != "Active")
            {
                return RedirectToAction("Login", "Account");
            }

            // Soft Delete: Keep Customer records in Database
            customer.account_status = "Deleted";

            _context.SaveChanges();

            // Clear login session
            HttpContext.Session.Clear();

            return RedirectToAction("Login", "Account");
        }


    }
}
