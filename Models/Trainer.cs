using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebProjeGym.Models
{
    public class Trainer
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Ad alanı zorunludur")]
        [StringLength(50)]
        [Display(Name = "Ad")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Soyad alanı zorunludur")]
        [StringLength(50)]
        [Display(Name = "Soyad")]
        public string LastName { get; set; }

        // Hesaplanmış özellik - veritabanına map edilmez
        [NotMapped]
        public string FullName => $"{FirstName} {LastName}";

        [StringLength(200)]
        [Display(Name = "Uzmanlık Alanları")]
        public string? Specialization { get; set; } // "kilo verme", "kas geliştirme", "yoga" vb.

        [StringLength(500)]
        [Display(Name = "Biyografi")]
        public string? Bio { get; set; }

        [Required(ErrorMessage = "Salon seçimi zorunludur")]
        [Display(Name = "Salon")]
        public int GymBranchId { get; set; }
        public GymBranch? GymBranch { get; set; }

        public ICollection<TrainerService> TrainerServices { get; set; } = new List<TrainerService>();
        public ICollection<TrainerAvailability> TrainerAvailabilities { get; set; } = new List<TrainerAvailability>();
        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    }
}


