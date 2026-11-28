
using System;
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

        //Profile Pic
        [StringLength(255)]
        public string? profile_photo { get; set; }

        [Required, StringLength(255)]
        public string password_hash { get; set; } = string.Empty;

        [Required, StringLength(20)]
        public string account_status { get; set; } = "Active";

        public DateTime created_at { get; set; } = DateTime.Now;

        // Login Security
        public int failed_login_attempts { get; set; } = 0;

        public DateTime? locked_until { get; set; }

        // Forgot Password
        [StringLength(255)]
        public string? reset_token { get; set; }

        public DateTime? reset_token_expiry { get; set; }
    }
}
