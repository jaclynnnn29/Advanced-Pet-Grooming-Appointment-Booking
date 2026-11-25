using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Advanced_Pet_Grooming.Models
{
    public class Announcement
    {
        [Key]
        public int AnnouncementId { get; set; }

        [Required(ErrorMessage = "Title is required")]
        [StringLength(150)]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Description is required")]
        public string Description { get; set; } = string.Empty;

        public string? ImagePath { get; set; }

        [NotMapped]
        public IFormFile? ImageFile { get; set; }

        [Required]
        public string Status { get; set; } = "Active"; // "Active" or "Inactive"

        [Required]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; } = DateTime.Now;
    }
}