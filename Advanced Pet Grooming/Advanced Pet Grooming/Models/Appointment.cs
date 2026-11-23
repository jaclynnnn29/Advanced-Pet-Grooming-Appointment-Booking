using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Advanced_Pet_Grooming.Models
{
    public class Appointment
    {
        [Key]
        public int appointment_id { get; set; }

        [Required]
        public int customer_id { get; set; }

        [Required]
        public int pet_id { get; set; }

        [Required]
        public int staff_id { get; set; }

        [Required, StringLength(20)]
        public string booking_type { get; set; } = "Online"; // Online, Walk-in

        [Required, StringLength(100)]
        public string service_package { get; set; } = string.Empty;

        [Required]
        public DateTime appointment_date { get; set; }

        [Required, StringLength(30)]
        public string time_slot { get; set; } = string.Empty;

        [Required, StringLength(20)]
        public string appointment_status { get; set; } = "Pending"; // Pending, Confirmed, Completed, Cancelled

        // Navigation properties
        [ForeignKey("customer_id")]
        public Customer? Customer { get; set; }

        [ForeignKey("pet_id")]
        public Pet? Pet { get; set; }

        [ForeignKey("staff_id")]
        public Staff? Staff { get; set; }

        public ICollection<Payment>? Payments { get; set; }
    }
}