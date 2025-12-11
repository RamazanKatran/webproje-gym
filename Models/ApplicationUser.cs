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

        [Range(100, 250)]
        public int? HeightCm { get; set; }

        [Range(30, 250)]
        public float? WeightKg { get; set; }

        [StringLength(200)]
        public string Goal { get; set; } // "kilo verme", "kas geliştirme" gibi
    }
}