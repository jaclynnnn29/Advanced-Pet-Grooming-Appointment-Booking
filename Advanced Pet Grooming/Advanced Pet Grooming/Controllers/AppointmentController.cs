using Advanced_Pet_Grooming.Data;
using Advanced_Pet_Grooming.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Advanced_Pet_Grooming.Controllers
{
    public class AppointmentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private const int MaxBookingsPerSlot = 5;

        public static readonly List<string> StandardTimeSlots = new()
        {
            "09:00 AM - 10:30 AM",
            "10:30 AM - 12:00 PM",
            "01:00 PM - 02:30 PM",
            "02:30 PM - 04:00 PM",
            "04:00 PM - 05:30 PM"
        };

        public AppointmentController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ==========================================
        // DFD 3.3: VIEW APPOINTMENTS (Customer & Staff/Admin)
        // ==========================================
        [HttpGet]
        public IActionResult Index(DateTime? filterDate, string? status)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            string? role = HttpContext.Session.GetString("UserRole");

            if (userId == null)
                return RedirectToAction("Login", "Account");

            var query = _context.Appointments
                .Include(a => a.Customer)
                .Include(a => a.Pet)
                .Include(a => a.Staff)
                .AsQueryable();

            if (role == "Customer")
            {
                query = query.Where(a => a.customer_id == userId.Value);
            }
            else if (filterDate.HasValue)
            {
                query = query.Where(a => a.appointment_date.Date == filterDate.Value.Date);
            }

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(a => a.appointment_status == status);
            }

            ViewBag.FilterDate = filterDate?.ToString("yyyy-MM-dd");
            ViewBag.SelectedStatus = status;
            return View(query.OrderByDescending(a => a.appointment_date).ThenBy(a => a.time_slot).ToList());
        }

        // ==========================================
        // LIVE SLOT AVAILABILITY (Max 5 per date + time_slot)
        // ==========================================
        [HttpGet]
        public IActionResult GetSlotAvailability(DateTime date)
        {
            var slotCounts = _context.Appointments
                .Where(a => a.appointment_date.Date == date.Date && a.appointment_status != "Cancelled")
                .GroupBy(a => a.time_slot)
                .Select(g => new { TimeSlot = g.Key, Count = g.Count() })
                .ToDictionary(x => x.TimeSlot, x => x.Count);

            var result = StandardTimeSlots.Select(slot =>
            {
                int booked = slotCounts.ContainsKey(slot) ? slotCounts[slot] : 0;
                return new
                {
                    timeSlot = slot,
                    bookedCount = booked,
                    remaining = Math.Max(0, MaxBookingsPerSlot - booked),
                    isFull = booked >= MaxBookingsPerSlot
                };
            });

            return Json(result);
        }

        // ==========================================
        // DFD 3.1: STAFF SEARCH CUSTOMER & PETS (Walk-in)
        // ==========================================
        [HttpGet]
        public IActionResult SearchCustomerPets(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return Json(new List<object>());

            query = query.Trim().ToLower();
            var customers = _context.Customers
                .Where(c => c.account_status == "Active" &&
                           (c.name.ToLower().Contains(query) ||
                            c.phone_no.Contains(query) ||
                            c.email.ToLower().Contains(query)))
                .Select(c => new
                {
                    customerId = c.customer_id,
                    name = c.name,
                    phone = c.phone_no,
                    pets = _context.Pets
                        .Where(p => p.customer_id == c.customer_id)
                        .Select(p => new
                        {
                            petId = p.pet_id,
                            petName = p.pet_name,
                            petType = p.pet_type,
                            sizeCategory = p.size_category
                        })
                        .ToList()
                })
                .Take(10)
                .ToList();

            return Json(customers);
        }

        // ==========================================
        // DFD 3.2: CREATE ONLINE & WALK-IN APPOINTMENT
        // ==========================================
        [HttpGet]
        public IActionResult Create()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            string? role = HttpContext.Session.GetString("UserRole");

            if (userId == null)
                return RedirectToAction("Login", "Account");

            ReloadCreateViewBags(role, userId.Value);
            return View(new Appointment { appointment_date = DateTime.Today.AddDays(1) });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Appointment model)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            string? role = HttpContext.Session.GetString("UserRole");

            if (userId == null)
                return RedirectToAction("Login", "Account");

            // Set booking_type & initial status per DFD 3.2 & Data Dictionary
            if (role == "Customer")
            {
                model.customer_id = userId.Value;
                model.booking_type = "Online";
                model.appointment_status = "Pending";
            }
            else
            {
                model.booking_type = "Walk-in";
                model.appointment_status = "Confirmed";
            }

            // Default staff_id if auto-assigned
            if (model.staff_id == 0)
            {
                var defaultStaff = _context.Staffs
                    .FirstOrDefault(s => s.account_status == "Active" && (s.role == "Groomer" || s.role == "Staff"));
                if (defaultStaff != null)
                {
                    model.staff_id = defaultStaff.staff_id;
                }
            }

            // Server-side guard: block booking if slot already has 5 active appointments
            int activeCount = _context.Appointments.Count(a =>
                a.appointment_date.Date == model.appointment_date.Date &&
                a.time_slot == model.time_slot &&
                a.appointment_status != "Cancelled");

            if (activeCount >= MaxBookingsPerSlot)
            {
                ViewBag.Error = $"The selected time slot ({model.time_slot}) is full (5/5 bookings). Please select another slot.";
                ReloadCreateViewBags(role, userId.Value);
                return View(model);
            }

            if (model.customer_id == 0 || model.pet_id == 0 ||
                string.IsNullOrWhiteSpace(model.time_slot) ||
                string.IsNullOrWhiteSpace(model.service_package))
            {
                ViewBag.Error = "Please select a pet, grooming package, date, and available time slot.";
                ReloadCreateViewBags(role, userId.Value);
                return View(model);
            }

            _context.Appointments.Add(model);
            _context.SaveChanges();

            TempData["Success"] = "Appointment booked successfully!";
            return RedirectToAction("Index");
        }

        // ==========================================
        // DFD 3.3: UPDATE / RESCHEDULE / CANCEL APPOINTMENT
        // ==========================================
        [HttpGet]
        public IActionResult Edit(int id)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            string? role = HttpContext.Session.GetString("UserRole");
            if (userId == null) return RedirectToAction("Login", "Account");

            var appointment = _context.Appointments
                .Include(a => a.Customer)
                .Include(a => a.Pet)
                .FirstOrDefault(a => a.appointment_id == id);

            if (appointment == null) return NotFound();
            if (role == "Customer" && appointment.customer_id != userId.Value) return Forbid();

            ViewBag.TimeSlots = StandardTimeSlots;
            ViewBag.Groomers = _context.Staffs.Where(s => s.account_status == "Active").ToList();
            return View(appointment);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, DateTime appointment_date, string time_slot, string service_package, string appointment_status, int? staff_id)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            string? role = HttpContext.Session.GetString("UserRole");
            if (userId == null) return RedirectToAction("Login", "Account");

            var appointment = _context.Appointments.FirstOrDefault(a => a.appointment_id == id);
            if (appointment == null) return NotFound();
            if (role == "Customer" && appointment.customer_id != userId.Value) return Forbid();

            if (appointment_status != "Cancelled")
            {
                int slotCount = _context.Appointments.Count(a =>
                    a.appointment_id != id &&
                    a.appointment_date.Date == appointment_date.Date &&
                    a.time_slot == time_slot &&
                    a.appointment_status != "Cancelled");

                if (slotCount >= MaxBookingsPerSlot)
                {
                    TempData["Error"] = $"Cannot reschedule to {time_slot} on {appointment_date:yyyy-MM-dd} — slot is full (5/5).";
                    return RedirectToAction("Edit", new { id });
                }
            }

            appointment.appointment_date = appointment_date;
            appointment.time_slot = time_slot;
            if (!string.IsNullOrWhiteSpace(service_package))
            {
                appointment.service_package = service_package;
            }

            if (role != "Customer")
            {
                appointment.appointment_status = appointment_status;
                if (staff_id.HasValue && staff_id.Value > 0)
                {
                    appointment.staff_id = staff_id.Value;
                }
            }

            _context.SaveChanges();
            TempData["Success"] = "Appointment updated successfully.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Cancel(int id)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            string? role = HttpContext.Session.GetString("UserRole");
            if (userId == null) return RedirectToAction("Login", "Account");

            var appointment = _context.Appointments.FirstOrDefault(a => a.appointment_id == id);
            if (appointment == null) return NotFound();
            if (role == "Customer" && appointment.customer_id != userId.Value) return Forbid();

            appointment.appointment_status = "Cancelled";
            _context.SaveChanges();

            TempData["Success"] = "Appointment cancelled and time slot released.";
            return RedirectToAction("Index");
        }

        private void ReloadCreateViewBags(string? role, int userId)
        {
            ViewBag.TimeSlots = StandardTimeSlots;
            ViewBag.Groomers = _context.Staffs
                .Where(s => s.account_status == "Active" && (s.role == "Groomer" || s.role == "Staff"))
                .ToList();

            if (role == "Customer")
            {
                ViewBag.Pets = _context.Pets.Where(p => p.customer_id == userId).ToList();
            }
        }
    }
}