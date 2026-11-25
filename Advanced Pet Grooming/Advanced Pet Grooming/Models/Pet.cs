using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Advanced_Pet_Grooming.Models
{
    public class Pet
    {
        [Key]
        [Column("PetID")]
        public int pet_id { get; set; }

        [Required]
        [Column("CustomerID")]
        public int customer_id { get; set; }

        [Required, StringLength(100)]
        [Column("PetName")]
        public string pet_name { get; set; } = string.Empty;

        [Required, StringLength(50)]
        [Column("PetType")]
        public string pet_type { get; set; } = string.Empty;

        [Column("Breed")]
        public string? breed { get; set; }

        [Required, StringLength(20)]
        [Column("Gender")]
        public string gender { get; set; } = string.Empty;

        [Required]
        [Column("Age")]
        public int age { get; set; }

        [Required]
        [Column("Weight", TypeName = "decimal(5,2)")]
        public decimal weight { get; set; }

        [Required, StringLength(30)]
        [Column("SizeCategory")]
        public string size_category { get; set; } = string.Empty;

        // Vaccination Information
        [Column("IsVaccinated")]
        public bool is_vaccinated { get; set; } = false;

        [StringLength(100)]
        [Column("VaccineName")]
        public string? vaccine_name { get; set; }

        [Column("VaccinationDate")]
        public DateTime? vaccination_date { get; set; }

        [Column("VaccinationExpiryDate")]
        public DateTime? vaccination_expiry_date { get; set; }

        // Medical Information
        [StringLength(500)]
        [Column("MedicalNotes")]
        public string? medical_record { get; set; }

        [Column("PetRemark")]
        public string? pet_remark { get; set; }

        // Navigation properties
        [ForeignKey("customer_id")]
        public Customer? Customer { get; set; }
        public ICollection<PetPhoto>? PetPhotos { get; set; }
        public ICollection<Appointment>? Appointments { get; set; }
    }
}
