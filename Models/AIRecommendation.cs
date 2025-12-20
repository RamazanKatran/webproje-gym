using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebProjeGym.Models
{
    public class AIRecommendation
    {
        public int Id { get; set; }

        [Required]
        public string ApplicationUserId { get; set; }
        public ApplicationUser ApplicationUser { get; set; }

        [Required]
        public RecommendationType RecommendationType { get; set; }

        [Required]
        [Column(TypeName = "nvarchar(max)")]
        public string Content { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Öneri alınırken kullanılan parametreler (geçmiş için referans)
        public int HeightCm { get; set; }
        public float WeightKg { get; set; }
        public int Age { get; set; }

        [StringLength(50)]
        public string BodyType { get; set; } = string.Empty;

        [StringLength(200)]
        public string Goal { get; set; } = string.Empty;
    }
}

