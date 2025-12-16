using System.ComponentModel.DataAnnotations;

namespace WebProjeGym.Models
{
    public class Trainer
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string FirstName { get; set; }

        [Required]
        [StringLength(50)]
        public string LastName { get; set; }

        [StringLength(200)]
        public string Specialization { get; set; } // "kilo verme", "kas geliştirme", "yoga" vb.

        [StringLength(500)]
        public string Bio { get; set; }

        [Required]
        public int GymBranchId { get; set; }
        public GymBranch GymBranch { get; set; }

        public ICollection<TrainerService> TrainerServices { get; set; }
        public ICollection<TrainerAvailability> TrainerAvailabilities { get; set; }
        public ICollection<Appointment> Appointments { get; set; }
    }
}


