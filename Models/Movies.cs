using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MvcMovie.Models
{
    public class Movie
    {
        [Key]
        [Required]
        [StringLength(255)]
        public string ImdbId { get; set; }

        [StringLength(255)]
        public string Title { get; set; }

        public int? Year { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal? Score { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal? ScoreAverage { get; set; }

        [StringLength(255)]
        public string Type { get; set; }

        public int? TmdbId { get; set; }
        public int? TraktId { get; set; }
        public int? MalId { get; set; }

        [NotMapped]
        public bool IsSaved { get; set; }

    }

    public class MovieDto
    {
        [Required]
        [StringLength(255)]
        public string ImdbId { get; set; }

        [Required]
        [StringLength(255)]
        public string Title { get; set; }

        public int? Year { get; set; }

        public decimal? Score { get; set; } // Nullable decimal for consistency
        public decimal? ScoreAverage { get; set; } // Nullable decimal for consistency

        [Required]
        [StringLength(255)]
        public string Type { get; set; }

        public int? TmdbId { get; set; } // Nullable
        public int? TraktId { get; set; } // Nullable
        public int? MalId { get; set; } // Nullable
    }
}
