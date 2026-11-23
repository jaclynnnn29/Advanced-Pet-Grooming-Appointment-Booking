using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Advanced_Pet_Grooming.Models
{
    public class Payment
    {
        [Key]
        public int payment_id { get; set; }

        [Required]
        public int appointment_id { get; set; }

        [Required]
        public int staff_id { get; set; }

        [Required]
        public string invoice_details { get; set; } = string.Empty;

        [Required, Column(TypeName = "decimal(10,2)")]
        public decimal total_amount { get; set; }

        [StringLength(100)]
        public string? transaction_id { get; set; }

        [Required, StringLength(20)]
        public string payment_status { get; set; } = "Unpaid"; // Unpaid, Paid, Voided

        public DateTime? paid_at { get; set; }

        // Navigation properties
        [ForeignKey("appointment_id")]
        public Appointment? Appointment { get; set; }

        [ForeignKey("staff_id")]
        public Staff? Staff { get; set; }
    }
}