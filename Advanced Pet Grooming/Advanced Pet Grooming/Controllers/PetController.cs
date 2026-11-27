
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Advanced_Pet_Grooming.Data;
using Advanced_Pet_Grooming.Models;

namespace Advanced_Pet_Grooming.Controllers
{
    public class PetController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public PetController(
            ApplicationDbContext context,
            IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // Get logged-in Customer ID
        private int? GetCustomerId()
        {
            return HttpContext.Session.GetInt32("UserId");
        }

        // Check Customer role
        private bool IsCustomer()
        {
            return HttpContext.Session.GetString("UserRole") == "Customer";
        }


        // Get pet belonging to current Customer
        private async Task<Pet?> GetCustomerPet(int id)
        {
            int? customerId = GetCustomerId();

            if (customerId == null)
            {
                return null;
            }

            return await _context.Pets
                .Include(p => p.PetPhotos)
                .FirstOrDefaultAsync(p =>
                    p.pet_id == id &&
                    p.customer_id == customerId.Value);
        }

        // Check uploaded photos
        private bool ValidatePhotos(List<IFormFile>? photos)
        {
            if (photos == null)
            {
                return true;
            }

            string[] allowedExtensions =
                { ".jpg", ".jpeg", ".png", ".webp" };

            foreach (var photo in photos)
            {
                if (photo == null || photo.Length == 0)
                {
                    continue;
                }

                string extension =
                    Path.GetExtension(photo.FileName).ToLowerInvariant();

                if (!allowedExtensions.Contains(extension))
                {
                    return false;
                }

                if (photo.Length > 5 * 1024 * 1024)
                {
                    return false;
                }
            }

            return true;
        }

        // Save pet photos
        private async Task<List<PetPhoto>> SavePhotos(
            int petId,
            List<IFormFile>? photos)
        {
            var savedPhotos = new List<PetPhoto>();

            if (photos == null || photos.Count == 0)
            {
                return savedPhotos;
            }

            string folderPath = Path.Combine(
                _environment.WebRootPath,
                "uploads",
                "pets");

            Directory.CreateDirectory(folderPath);

            foreach (var photo in photos)
            {
                if (photo == null || photo.Length == 0)
                {
                    continue;
                }

                string extension =
                    Path.GetExtension(photo.FileName).ToLowerInvariant();

                string fileName = Guid.NewGuid().ToString("N") + extension;

                string fullPath = Path.Combine(folderPath, fileName);

                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await photo.CopyToAsync(stream);
                }

                savedPhotos.Add(new PetPhoto
                {
                    PetID = petId,
                    PhotoPath = "/uploads/pets/" + fileName
                });
            }

            return savedPhotos;
        }

        // Delete photo file
        private void DeletePhotoFile(string? photoPath)
        {
            if (string.IsNullOrWhiteSpace(photoPath))
            {
                return;
            }

            string fileName = Path.GetFileName(photoPath);

            string fullPath = Path.Combine(
                _environment.WebRootPath,
                "uploads",
                "pets",
                fileName);

            if (System.IO.File.Exists(fullPath))
            {
                System.IO.File.Delete(fullPath);
            }
        }

        // GET: Pet/Index
        public async Task<IActionResult> Index(string? searchString)
        {
            if (!IsCustomer())
            {
                return Forbid();
            }

            int? customerId = GetCustomerId();

            if (customerId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var pets = _context.Pets
                .Include(p => p.PetPhotos)
                .Where(p => p.customer_id == customerId.Value);

            // Search pet name
            if (!string.IsNullOrWhiteSpace(searchString))
            {
                pets = pets.Where(p =>
                    p.pet_name.Contains(searchString));
            }

            var petList = await pets
                .OrderByDescending(p => p.pet_id)
                .ToListAsync();

            ViewBag.SearchString = searchString;

            return View(petList);
        }

        // GET: Pet/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            if (!IsCustomer())
            {
                return Forbid();
            }

            var pet = await GetCustomerPet(id.Value);

            if (pet == null)
            {
                return NotFound();
            }

            return View(pet);
        }

        // GET: Pet/Create
        public IActionResult Create()
        {
            if (!IsCustomer())
            {
                return Forbid();
            }

            if (GetCustomerId() == null)
            {
                return RedirectToAction("Login", "Account");
            }

            return View();
        }

        // POST: Pet/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            Pet pet,
            List<IFormFile>? photos)
        {
            if (!IsCustomer())
            {
                return Forbid();
            }

            int? customerId = GetCustomerId();

            if (customerId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Remove fields that are calculated by the system
            ModelState.Remove("Customer");
            ModelState.Remove("PetPhotos");
            ModelState.Remove("size_category");


            // Check vaccination information
            bool hasVaccineName =
                !string.IsNullOrWhiteSpace(pet.vaccine_name);

            bool hasVaccinationDate =
                pet.vaccination_date.HasValue;

            bool hasExpiryDate =
                pet.vaccination_expiry_date.HasValue;

            // All vaccination information is empty
            if (!hasVaccineName && !hasVaccinationDate && !hasExpiryDate)
            {
                pet.is_vaccinated = false;
            }

            // All vaccination information is provided
            else if (hasVaccineName && hasVaccinationDate && hasExpiryDate)
            {
                pet.is_vaccinated = true;

                if (pet.vaccination_expiry_date is DateTime expiryDate &&
                    pet.vaccination_date is DateTime vaccinationDate &&
                    expiryDate < vaccinationDate)
                {
                    ModelState.AddModelError(
                        "vaccination_expiry_date",
                        "Expiry date cannot be earlier than vaccination date.");
                }
            }

            // Some information is missing
            else
            {
                ModelState.AddModelError(
                    "vaccine_name",
                    "Please provide complete vaccination information.");
            }


            // Check photos
            if (!ValidatePhotos(photos))
            {
                ModelState.AddModelError(
                    "photos",
                    "Only JPG, JPEG, PNG and WEBP images up to 5MB each are allowed.");
            }

            // Check weight
            if (pet.weight <= 0)
            {
                ModelState.AddModelError(
                    "weight",
                    "Weight must be greater than zero.");
            }

            // Check age
            if (pet.age < 0)
            {
                ModelState.AddModelError(
                    "age",
                    "Age cannot be negative.");
            }

            if (!ModelState.IsValid)
            {
                return View(pet);
            }

            // Assign Customer ID automatically
            pet.customer_id = customerId.Value;

            // Pet Size is no longer calculated
            pet.size_category = "Not Specified";

            pet.PetPhotos = new List<PetPhoto>();

            _context.Pets.Add(pet);

            await _context.SaveChangesAsync();/////////////

            // Save uploaded photos
            var savedPhotos = await SavePhotos(pet.pet_id, photos);

            if (savedPhotos.Count > 0)
            {
                _context.PetPhotos.AddRange(savedPhotos);
                await _context.SaveChangesAsync();
            }

            TempData["SuccessMessage"] = "Pet added successfully!";

            return RedirectToAction(nameof(Index));
        }

        // GET: Pet/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            if (!IsCustomer())
            {
                return Forbid();
            }

            var pet = await GetCustomerPet(id.Value);

            if (pet == null)
            {
                return NotFound();
            }

            return View(pet);
        }

        // POST: Pet/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            Pet pet,
            List<IFormFile>? photos)
        {
            if (!IsCustomer())
            {
                return Forbid();
            }

            var existingPet = await GetCustomerPet(id);

            if (existingPet == null)
            {
                return NotFound();
            }

            ModelState.Remove("Customer");
            ModelState.Remove("PetPhotos");
            ModelState.Remove("size_category");

            // Check vaccination information
            bool hasVaccineName =
                !string.IsNullOrWhiteSpace(pet.vaccine_name);

            bool hasVaccinationDate =
                pet.vaccination_date.HasValue;

            bool hasExpiryDate =
                pet.vaccination_expiry_date.HasValue;

            // All vaccination information is empty
            if (!hasVaccineName && !hasVaccinationDate && !hasExpiryDate)
            {
                pet.is_vaccinated = false;
            }

            // All vaccination information is provided
            else if (hasVaccineName && hasVaccinationDate && hasExpiryDate)
            {
                pet.is_vaccinated = true;

                if (pet.vaccination_expiry_date is DateTime expiryDate &&
                    pet.vaccination_date is DateTime vaccinationDate &&
                    expiryDate < vaccinationDate)
                {
                    ModelState.AddModelError(
                        "vaccination_expiry_date",
                        "Expiry date cannot be earlier than vaccination date.");
                }
            }

            // Some information is missing
            else
            {
                ModelState.AddModelError(
                    "vaccine_name",
                    "Please provide complete vaccination information.");
            }


            if (!ValidatePhotos(photos))
            {
                ModelState.AddModelError(
                    "photos",
                    "Only JPG, JPEG, PNG and WEBP images up to 5MB each are allowed.");
            }

            if (pet.weight <= 0)
            {
                ModelState.AddModelError(
                    "weight",
                    "Weight must be greater than zero.");
            }

            if (pet.age < 0)
            {
                ModelState.AddModelError(
                    "age",
                    "Age cannot be negative.");
            }

            if (!ModelState.IsValid)
            {
                pet.pet_id = id;
                pet.customer_id = existingPet.customer_id;
                pet.PetPhotos = existingPet.PetPhotos;

                return View(pet);
            }

            // Update pet information
            existingPet.pet_name = pet.pet_name;
            existingPet.pet_type = pet.pet_type;
            existingPet.breed = pet.breed;
            existingPet.gender = pet.gender;
            existingPet.age = pet.age;
            existingPet.weight = pet.weight;

            // Pet Size is no longer calculated
            existingPet.size_category = "Not Specified";

            existingPet.is_vaccinated = pet.is_vaccinated;
            existingPet.vaccine_name = pet.vaccine_name;
            existingPet.vaccination_date = pet.vaccination_date;
            existingPet.vaccination_expiry_date =
                pet.vaccination_expiry_date;

            existingPet.medical_record = pet.medical_record;
            existingPet.pet_remark = pet.pet_remark;

            // Add new photos without deleting old photos
            var newPhotos = await SavePhotos(id, photos);

            if (newPhotos.Count > 0)
            {
                _context.PetPhotos.AddRange(newPhotos);
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Pet information updated successfully!";

            return RedirectToAction(nameof(Index));
        }

        // GET: Pet/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            if (!IsCustomer())
            {
                return Forbid();
            }

            var pet = await GetCustomerPet(id.Value);

            if (pet == null)
            {
                return NotFound();
            }

            return View(pet);
        }

        // POST: Pet/Delete/5
        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!IsCustomer())
            {
                return Forbid();
            }

            var pet = await GetCustomerPet(id);

            if (pet == null)
            {
                return NotFound();
            }

            // Delete pet photos
            if (pet.PetPhotos != null)
            {
                foreach (var photo in pet.PetPhotos)
                {
                    DeletePhotoFile(photo.PhotoPath);
                }

                _context.PetPhotos.RemoveRange(pet.PetPhotos);
            }

            // Delete pet
            _context.Pets.Remove(pet);

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Pet deleted successfully!";

            return RedirectToAction(nameof(Index));
        }


        // POST: Pet/UploadPhoto
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadPhoto(
            int id,
            List<IFormFile>? photos)
        {
            if (!IsCustomer())
            {
                return Forbid();
            }

            // Check whether this pet belongs to the logged-in Customer
            var pet = await GetCustomerPet(id);

            if (pet == null)
            {
                return NotFound();
            }

            // Check whether photos were selected
            if (photos == null || !photos.Any(p => p.Length > 0))
            {
                TempData["ErrorMessage"] = "Please select at least one photo.";
                return RedirectToAction(nameof(Edit), new { id });
            }

            // Validate photos
            if (!ValidatePhotos(photos))
            {
                TempData["ErrorMessage"] =
                    "Only JPG, JPEG, PNG and WEBP images up to 5MB each are allowed.";

                return RedirectToAction(nameof(Details), new { id });
            }

            // Save new photos
            var savedPhotos = await SavePhotos(id, photos);

            if (savedPhotos.Count > 0)
            {
                _context.PetPhotos.AddRange(savedPhotos);
                await _context.SaveChangesAsync();
            }

            TempData["SuccessMessage"] = "Pet photos uploaded successfully!";

            return RedirectToAction(nameof(Details), new { id });
        }


        // POST: Pet/DeletePhoto
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePhoto(
            int id,
            int photoId)
        {
            if (!IsCustomer())
            {
                return Forbid();
            }

            // Check whether this pet belongs to the logged-in Customer
            var pet = await GetCustomerPet(id);

            if (pet == null)
            {
                return NotFound();
            }

            // Find the selected photo
            var photo = pet.PetPhotos?
                .FirstOrDefault(p => p.PetPhotoID == photoId);

            if (photo == null)
            {
                return NotFound();
            }

            // Remove photo from database
            _context.PetPhotos.Remove(photo);
            await _context.SaveChangesAsync();

            // Delete photo from wwwroot/uploads/pets
            DeletePhotoFile(photo.PhotoPath);

            TempData["SuccessMessage"] = "Photo deleted successfully!";

            return RedirectToAction(nameof(Details), new { id });
        }


    }

}
