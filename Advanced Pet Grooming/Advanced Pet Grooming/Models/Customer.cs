using System.ComponentModel.DataAnnotations;

namespace Advanced_Pet_Grooming.Models
{
    public class Customer
    {
        [Key]
        public int customer_id { get; set; }

        [Required, StringLength(100)]
        public string name { get; set; } = string.Empty;

        [Required, EmailAddress, StringLength(100)]
        public string email { get; set; } = string.Empty;

        [Required, StringLength(20)]
        public string phone_no { get; set; } = string.Empty;

        [Required, StringLength(255)]
        public string password_hash { get; set; } = string.Empty;

        [Required, StringLength(20)]
        public string account_status { get; set; } = "Active";

        public DateTime created_at { get; set; } = DateTime.Now;

        // Navigation properties
        public ICollection<Pet>? Pets { get; set; }
        public ICollection<Appointment>? Appointments { get; set; }
    }
}
