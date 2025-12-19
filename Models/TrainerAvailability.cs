using System.ComponentModel.DataAnnotations;

namespace WebProjeGym.Models
{
    public class TrainerAvailability
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Antrenör seçimi zorunludur")]
        [Display(Name = "Antrenör")]
        public int TrainerId { get; set; }
        public Trainer? Trainer { get; set; }

        [Required(ErrorMessage = "Hafta günü seçimi zorunludur")]
        [Display(Name = "Hafta Günü")]
        public DayOfWeek DayOfWeek { get; set; }

        [Required(ErrorMessage = "Başlangıç saati zorunludur")]
        [Display(Name = "Başlangıç Saati")]
        public TimeSpan StartTime { get; set; }

        [Required(ErrorMessage = "Bitiş saati zorunludur")]
        [Display(Name = "Bitiş Saati")]
        public TimeSpan EndTime { get; set; }
    }
}


