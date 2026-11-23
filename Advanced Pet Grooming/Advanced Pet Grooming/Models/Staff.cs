using System.ComponentModel.DataAnnotations;

namespace Advanced_Pet_Grooming.Models
{
    public class Staff
    {
        [Key]
        public int staff_id { get; set; }

        [Required, StringLength(100)]
        public string staff_name { get; set; } = string.Empty;

        [Required, StringLength(30)]
        public string role { get; set; } = "Staff"; // Admin, Groomer, Staff

        [Required, EmailAddress, StringLength(100)]
        public string email { get; set; } = string.Empty;

        [Required, StringLength(20)]
        public string phone_no { get; set; } = string.Empty;

        [Required, StringLength(255)]
        public string password_hash { get; set; } = string.Empty;

        [Required, StringLength(20)]
        public string account_status { get; set; } = "Active";

        // Navigation properties
        public ICollection<Appointment>? Appointments { get; set; }
        public ICollection<Announcement>? Announcements { get; set; }
        public ICollection<Payment>? Payments { get; set; }
    }
}