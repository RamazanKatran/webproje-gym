using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebProjeGym.Models
{
    public class GymBranch
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Ad alanı zorunludur")]
        [StringLength(100)]
        [Display(Name = "Ad")]
        public string Name { get; set; }

        [StringLength(200)]
        [Display(Name = "Adres")]
        public string? Address { get; set; }

        [StringLength(500)]
        [Display(Name = "Açıklama")]
        public string? Description { get; set; }

        // Çalışma saatleri - basit tutmak için string, istenirse TimeOnly/TimeSpan'e çevrilebilir
        [StringLength(20)]
        [Display(Name = "Açılış Saati")]
        public string? OpeningTime { get; set; }

        [StringLength(20)]
        [Display(Name = "Kapanış Saati")]
        public string? ClosingTime { get; set; }

        public ICollection<Service> Services { get; set; } = new List<Service>();
        public ICollection<Trainer> Trainers { get; set; } = new List<Trainer>();
    }
}


