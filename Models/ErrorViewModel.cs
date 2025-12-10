using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebProjeGym.Models
{

    public class GymBranch
    {
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string Name { get; set; }

        [StringLength(200)]
        public string Address { get; set; }

        [StringLength(100)]
        public string WorkingHours { get; set; } // "09:00 - 22:00" gibi

        public ICollection<Service> Services { get; set; }
        public ICollection<Trainer> Trainers { get; set; }
        public ICollection<Appointment> Appointments { get; set; }
    }

    public class Service
    {
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string Name { get; set; } // Fitness, Yoga, Pilates...

        [StringLength(200)]
        public string Description { get; set; }

        [Range(10, 300)]
        public int DurationMinutes { get; set; }

        [Range(0, 10000)]
        public decimal Price { get; set; }

        public int GymBranchId { get; set; }
        public GymBranch GymBranch { get; set; }

        public ICollection<TrainerService> TrainerServices { get; set; }
        public ICollection<Appointment> Appointments { get; set; }
    }

    public class ErrorViewModel
    {
        public string? RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }

    public class Trainer
    {
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string FullName { get; set; }

        [StringLength(200)]
        public string Specialties { get; set; } // "Kas geliþtirme, kilo verme" gibi

        public int GymBranchId { get; set; }
        public GymBranch GymBranch { get; set; }

        public ICollection<TrainerService> TrainerServices { get; set; }
        public ICollection<TrainerAvailability> Availabilities { get; set; }
        public ICollection<Appointment> Appointments { get; set; }
    }

    public class TrainerService
    {
        public int TrainerId { get; set; }
        public Trainer Trainer { get; set; }

        public int ServiceId { get; set; }
        public Service Service { get; set; }
    }

    public class TrainerAvailability
    {
        public int Id { get; set; }

        public int TrainerId { get; set; }
        public Trainer Trainer { get; set; }

        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
    }

    public class Appointment
    {
        public int Id { get; set; }

        [Required]
        public int GymBranchId { get; set; }
        public GymBranch GymBranch { get; set; }

        [Required]
        public int ServiceId { get; set; }
        public Service Service { get; set; }

        [Required]
        public int TrainerId { get; set; }
        public Trainer Trainer { get; set; }

        [Required]
        public string MemberId { get; set; } // Identity User Id
        public ApplicationUser Member { get; set; }

        [Required]
        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }

        [Required]
        public string Status { get; set; } = "Pending"; // Pending/Approved/Cancelled
    }
}
