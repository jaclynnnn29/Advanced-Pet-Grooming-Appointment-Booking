using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Advanced_Pet_Grooming.Models
{
    public class Announcement
    {
        [Key]
        public int AnnouncementId { get; set; }

        [Required(ErrorMessage = "Title is required")]
        [StringLength(200, MinimumLength = 20, 
            ErrorMessage = "Title must be between 20 and 300 characters.")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Description is required")]
        [MinLength(160, ErrorMessage = "Description must be at least 160 characters.")]
        public string Description { get; set; } = string.Empty;

        public string? ImagePath { get; set; }

        [NotMapped]
        public IFormFile? ImageFile { get; set; }

        [Required(ErrorMessage = "Status is required.")]
        public string Status { get; set; } = "Active"; // "Active" or "Inactive"

        [Required(ErrorMessage = "Publish Start Date is required.")]
        [DataType(DataType.Date)]
        [Display(Name = "Publish Start Date")]
        public DateTime StartDate { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "Publish End Date is required.")]
        [DataType(DataType.Date)]
        [Display(Name = "Publish End Date")]
        public DateTime EndDate { get; set; } = DateTime.Today.AddDays(7);
        public DateTime Date { get; set; } = DateTime.Now;
    }
}