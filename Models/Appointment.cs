using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebProjeGym.Models
{
    public enum AppointmentStatus
    {
        Pending = 0,
        Approved = 1,
        Cancelled = 2
    }

    public class Appointment
    {
        public int Id { get; set; }

        [Required]
        public int MemberProfileId { get; set; }
        public MemberProfile MemberProfile { get; set; }

        [Required]
        public int TrainerId { get; set; }
        public Trainer Trainer { get; set; }

        [Required]
        public int ServiceId { get; set; }
        public Service Service { get; set; }

        [Required]
        public DateTime StartDateTime { get; set; }

        [Range(10, 300)]
        public int DurationMinutes { get; set; }

        [Range(0, 10000)]
        public decimal Price { get; set; }

        [Required]
        public AppointmentStatus Status { get; set; } = AppointmentStatus.Pending;

        // Hesaplanmış özellik - veritabanına map edilmez
        public DateTime EndDateTime => StartDateTime.AddMinutes(DurationMinutes);

        // View için yardımcı property'ler - veritabanına map edilmez
        [NotMapped]
        [Display(Name = "Tarih")]
        public DateTime? AppointmentDate { get; set; }

        [NotMapped]
        [Display(Name = "Saat")]
        public TimeSpan? AppointmentTime { get; set; }
    }
}


