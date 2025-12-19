using System.ComponentModel.DataAnnotations;

namespace WebProjeGym.Models
{
    public class Service
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Ad alanı zorunludur")]
        [StringLength(100)]
        [Display(Name = "Ad")]
        public string Name { get; set; }

        [Range(10, 300, ErrorMessage = "Süre 10-300 dakika arasında olmalıdır")]
        [Display(Name = "Süre (Dakika)")]
        public int DurationMinutes { get; set; } // Hizmet süresi (dakika)

        [Range(0, 10000, ErrorMessage = "Ücret 0-10000 arasında olmalıdır")]
        [Display(Name = "Ücret")]
        public decimal Price { get; set; }

        [StringLength(500)]
        [Display(Name = "Açıklama")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Salon seçimi zorunludur")]
        [Display(Name = "Salon")]
        public int GymBranchId { get; set; }
        public GymBranch? GymBranch { get; set; }

        public ICollection<TrainerService> TrainerServices { get; set; } = new List<TrainerService>();
        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    }
}


