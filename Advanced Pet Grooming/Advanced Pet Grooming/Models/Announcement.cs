using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Advanced_Pet_Grooming.Models
{
    public class Announcement
    {
        [Key]
        public int announcement_id { get; set; }

        [Required]
        public int staff_id { get; set; }

        [Required, StringLength(150)]
        public string title { get; set; } = string.Empty;

        [Required]
        public string content { get; set; } = string.Empty;

        public DateTime publish_date { get; set; } = DateTime.Now;

        [Required, StringLength(20)]
        public string status { get; set; } = "Active"; // Active, Archived

        // Navigation property
        [ForeignKey("staff_id")]
        public Staff? Staff { get; set; }
    }
}