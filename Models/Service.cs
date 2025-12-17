using System.ComponentModel.DataAnnotations;

namespace WebProjeGym.Models
{
    public class Service
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Range(10, 300)]
        public int DurationMinutes { get; set; } // Hizmet süresi (dakika)

        [Range(0, 10000)]
        public decimal Price { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        public int GymBranchId { get; set; }
        public GymBranch GymBranch { get; set; }

        public ICollection<TrainerService> TrainerServices { get; set; }
        public ICollection<Appointment> Appointments { get; set; }
    }
}


