using System.ComponentModel.DataAnnotations;

namespace WebProjeGym.Models
{
    public class AIRecommendationRequest
    {
        [Required(ErrorMessage = "Boy bilgisi gereklidir")]
        [Range(100, 250, ErrorMessage = "Boy 100-250 cm arasında olmalıdır")]
        [Display(Name = "Boy (cm)")]
        public int HeightCm { get; set; }

        [Required(ErrorMessage = "Kilo bilgisi gereklidir")]
        [Range(30, 250, ErrorMessage = "Kilo 30-250 kg arasında olmalıdır")]
        [Display(Name = "Kilo (kg)")]
        public float WeightKg { get; set; }

        [Required(ErrorMessage = "Yaş bilgisi gereklidir")]
        [Range(10, 100, ErrorMessage = "Yaş 10-100 arasında olmalıdır")]
        [Display(Name = "Yaş")]
        public int Age { get; set; }

        [Required(ErrorMessage = "Vücut tipi seçimi gereklidir")]
        [StringLength(50, ErrorMessage = "Vücut tipi en fazla 50 karakter olabilir")]
        [Display(Name = "Vücut Tipi")]
        public string BodyType { get; set; } = string.Empty;

        [Required(ErrorMessage = "Hedef bilgisi gereklidir")]
        [StringLength(200, ErrorMessage = "Hedef en fazla 200 karakter olabilir")]
        [Display(Name = "Hedef")]
        public string Goal { get; set; } = string.Empty;

        [Required(ErrorMessage = "Öneri tipi seçimi gereklidir")]
        [Display(Name = "Öneri Tipi")]
        public RecommendationType RecommendationType { get; set; }
    }

    public enum RecommendationType
    {
        [Display(Name = "Egzersiz Programı")]
        Exercise = 1,
        [Display(Name = "Beslenme Programı")]
        Diet = 2
    }
}

