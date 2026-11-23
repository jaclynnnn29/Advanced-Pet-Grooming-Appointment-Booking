using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Advanced_Pet_Grooming.Models
{
    public class Pet
    {
        [Key]
        public int pet_id { get; set; }

        [Required]
        public int customer_id { get; set; }

        [Required, StringLength(100)]
        public string pet_name { get; set; } = string.Empty;

        [Required, StringLength(50)]
        public string pet_type { get; set; } = string.Empty;

        [StringLength(100)]
        public string? breed { get; set; }

        [Required, StringLength(30)]
        public string size_category { get; set; } = string.Empty;

        public string? vaccination_record { get; set; }
        public string? medical_record { get; set; }
        public string? pet_remark { get; set; }

        // Navigation properties
        [ForeignKey("customer_id")]
        public Customer? Customer { get; set; }
        public ICollection<Appointment>? Appointments { get; set; }
    }
}
