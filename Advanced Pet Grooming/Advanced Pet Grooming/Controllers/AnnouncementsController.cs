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

        private bool IsAdmin()
        {
            return HttpContext.Session.GetString("UserRole") == "Admin";
        }

        // Automatically sync Scheduled / Active / Expired statuses based on today's date
        private async Task SyncAnnouncementStatusesAsync()
        {
            var today = DateTime.Today;
            var all = await _context.Announcements.ToListAsync();
            bool hasChanges = false;

            foreach (var item in all)
            {
                // Keep manual "Inactive" overrides untouched
                if (item.Status == "Inactive") continue;

                string expectedStatus;
                if (item.EndDate.Date < today)
                {
                    expectedStatus = "Expired";
                }
                else if (item.StartDate.Date > today)
                {
                    expectedStatus = "Scheduled";
                }
                else
                {
                    expectedStatus = "Active";
                }

                if (item.Status != expectedStatus)
                {
                    item.Status = expectedStatus;
                    hasChanges = true;
                }
            }

            if (hasChanges)
            {
                await _context.SaveChangesAsync();
            }
        }

        // View Announcements (Admin sees all; Public/Customer/Staff see currently Active only)
        public async Task<IActionResult> Index(string? searchKeyword)
        {
            await SyncAnnouncementStatusesAsync();

            bool isAdmin = IsAdmin();
            var today = DateTime.Today;
            var query = _context.Announcements.AsQueryable();

            if (!isAdmin)
            {
                query = query.Where(a =>
                    a.Status == "Active" &&
                    a.StartDate.Date <= today &&
                    a.EndDate.Date >= today);
            }

            if (!string.IsNullOrWhiteSpace(searchKeyword))
            {
                query = query.Where(a => a.Title.Contains(searchKeyword) || a.Description.Contains(searchKeyword));
            }

            var list = await query.OrderByDescending(a => a.StartDate).ToListAsync();

            if (!list.Any())
            {
                ViewBag.Notice = "No announcements found.";
            }

            ViewBag.SearchKeyword = searchKeyword;
            ViewBag.IsAdmin = isAdmin;
            return View(list);
        }

        // Create Announcement (GET) - Admin Only
        public IActionResult Create()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");
            return View(new Announcement
            {
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddDays(7),
                Status = "Active"
            });
        }

        // Create Announcement (POST) - Admin Only
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Announcement model)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            if (model.EndDate.Date < model.StartDate.Date)
            {
                ModelState.AddModelError("EndDate", "Publish End Date cannot be earlier than Publish Start Date.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Set initial status automatically unless manually set to Inactive
            if (model.Status != "Inactive")
            {
                var today = DateTime.Today;
                if (model.EndDate.Date < today) model.Status = "Expired";
                else if (model.StartDate.Date > today) model.Status = "Scheduled";
                else model.Status = "Active";
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

            TempData["SuccessMessage"] = "Announcement saved successfully!";
            return RedirectToAction(nameof(Index));
        }

        // Update Announcement (GET) - Admin Only
        public async Task<IActionResult> Edit(int? id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");
            if (id == null) return NotFound();

            var announcement = await _context.Announcements.FindAsync(id);
            if (announcement == null) return NotFound();

            return View(announcement);
        }

        // Update Announcement (POST) - Admin Only
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Announcement model)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");
            if (id != model.AnnouncementId) return NotFound();

            if (model.EndDate.Date < model.StartDate.Date)
            {
                ModelState.AddModelError("EndDate", "Publish End Date cannot be earlier than Publish Start Date.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var existing = await _context.Announcements.FindAsync(id);
            if (existing == null) return NotFound();

            existing.Title = model.Title;
            existing.Description = model.Description;
            existing.StartDate = model.StartDate;
            existing.EndDate = model.EndDate;

            if (model.Status == "Inactive")
            {
                existing.Status = "Inactive";
            }
            else
            {
                var today = DateTime.Today;
                if (model.EndDate.Date < today) existing.Status = "Expired";
                else if (model.StartDate.Date > today) existing.Status = "Scheduled";
                else existing.Status = "Active";
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
                existing.ImagePath = "/images/announcements/" + uniqueFileName;
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Announcement updated successfully!";
            return RedirectToAction(nameof(Index));
        }

        // Delete Announcement (POST) - Admin Only
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