using Advanced_Pet_Grooming.Data;
using Advanced_Pet_Grooming.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Advanced_Pet_Grooming.Controllers
{
    public class AnnouncementsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public AnnouncementsController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // Helper: Matches "UserRole" set in AccountController
        private bool IsAdmin()
        {
            return HttpContext.Session.GetString("UserRole") == "Admin";
        }

        // FR 5.2: View Announcements (Admin sees all; Public/Customer/Staff see Active only + Search)
        public async Task<IActionResult> Index(string? searchKeyword)
        {
            bool isAdmin = IsAdmin();
            var query = _context.Announcements.AsQueryable();

            if (!isAdmin)
            {
                query = query.Where(a => a.Status == "Active");
            }

            if (!string.IsNullOrWhiteSpace(searchKeyword))
            {
                query = query.Where(a => a.Title.Contains(searchKeyword) || a.Description.Contains(searchKeyword));
            }

            var list = await query.OrderByDescending(a => a.Date).ToListAsync();

            if (!list.Any())
            {
                ViewBag.Notice = "No announcements found.";
            }

            ViewBag.SearchKeyword = searchKeyword;
            ViewBag.IsAdmin = isAdmin;
            return View(list);
        }

        // FR 5.1: Create Announcement (GET) - Admin Only
        public IActionResult Create()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");
            return View(new Announcement());
        }

        // FR 5.1: Create Announcement (POST) - Admin Only
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Announcement model)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (model.ImageFile != null && model.ImageFile.Length > 0)
            {
                string uploadsFolder = Path.Combine(_env.WebRootPath, "images", "announcements");
                Directory.CreateDirectory(uploadsFolder);
                string uniqueFileName = Guid.NewGuid() + "_" + Path.GetFileName(model.ImageFile.FileName);
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.ImageFile.CopyToAsync(stream);
                }
                model.ImagePath = "/images/announcements/" + uniqueFileName;
            }

            _context.Announcements.Add(model);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Announcement published successfully!";
            return RedirectToAction(nameof(Index));
        }

        // FR 5.3: Update Announcement (GET) - Admin Only
        public async Task<IActionResult> Edit(int? id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");
            if (id == null) return NotFound();

            var announcement = await _context.Announcements.FindAsync(id);
            if (announcement == null) return NotFound();

            return View(announcement);
        }

        // FR 5.3: Update Announcement (POST) - Admin Only
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Announcement model)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");
            if (id != model.AnnouncementId) return NotFound();

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var existing = await _context.Announcements.FindAsync(id);
            if (existing == null) return NotFound();

            existing.Title = model.Title;
            existing.Description = model.Description;
            existing.Status = model.Status;
            existing.Date = model.Date;

            if (model.ImageFile != null && model.ImageFile.Length > 0)
            {
                string uploadsFolder = Path.Combine(_env.WebRootPath, "images", "announcements");
                Directory.CreateDirectory(uploadsFolder);
                string uniqueFileName = Guid.NewGuid() + "_" + Path.GetFileName(model.ImageFile.FileName);
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.ImageFile.CopyToAsync(stream);
                }
                existing.ImagePath = "/images/announcements/" + uniqueFileName;
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Announcement updated successfully!";
            return RedirectToAction(nameof(Index));
        }

        // FR 5.4: Delete Announcement (POST) - Admin Only
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            var announcement = await _context.Announcements.FindAsync(id);
            if (announcement == null)
            {
                TempData["ErrorMessage"] = "Announcement not found or already deleted.";
                return RedirectToAction(nameof(Index));
            }

            _context.Announcements.Remove(announcement);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Announcement deleted successfully!";
            return RedirectToAction(nameof(Index));
        }
    }
}