
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
    }
}
