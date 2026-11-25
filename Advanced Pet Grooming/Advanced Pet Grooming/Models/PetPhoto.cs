
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Advanced_Pet_Grooming.Models
{
    public class PetPhoto
    {
        [Key]
        public int PetPhotoID { get; set; }

        [Required]
        public int PetID { get; set; }

        [Required, StringLength(500)]
        public string PhotoPath { get; set; } = string.Empty;

        // Navigation Property
        [ForeignKey("PetID")]
        public Pet? Pet { get; set; }
    }
}
