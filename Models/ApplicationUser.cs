using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace WebProjeGym.Models
{
    public class ApplicationUser : IdentityUser
    {
        public MemberProfile MemberProfile { get; set; }
    }

    public class MemberProfile
    {
        public int Id { get; set; }

        [Required]
        public string ApplicationUserId { get; set; }
        public ApplicationUser ApplicationUser { get; set; }

        [Range(100, 250, ErrorMessage = "Boy 100-250 cm arasında olmalıdır")]
        [Display(Name = "Boy (cm)")]
        public int? HeightCm { get; set; }

        [Range(30, 250, ErrorMessage = "Kilo 30-250 kg arasında olmalıdır")]
        [Display(Name = "Kilo (kg)")]
        public float? WeightKg { get; set; }

        [StringLength(200)]
        [Display(Name = "Hedef")]
        public string? Goal { get; set; } // "kilo verme", "kas geliştirme" gibi
    }
    public class Temp
    {
        public int Id { get; set; }
    }
}
