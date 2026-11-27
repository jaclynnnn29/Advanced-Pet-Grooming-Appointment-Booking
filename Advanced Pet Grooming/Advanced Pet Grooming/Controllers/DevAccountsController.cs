using Advanced_Pet_Grooming.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Advanced_Pet_Grooming.Controllers
{
    public class DevAccountsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DevAccountsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /DevAccounts
        public async Task<IActionResult> Index()
        {
            var customers = await _context.Customers
                .AsNoTracking()
                .ToListAsync();

            var staffs = await _context.Staffs
                .AsNoTracking()
                .ToListAsync();

            ViewBag.Customers = customers;
            ViewBag.Staffs = staffs;

            return View();
        }
    }
}