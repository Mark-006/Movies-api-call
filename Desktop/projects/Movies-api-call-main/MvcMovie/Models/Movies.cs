using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace MvcMovie.Models
{
    public class Movie
    {
        [Key]
        [StringLength(255)]
        public string ImdbId { get; set; }

        [Required]
        [StringLength(255)]
        public string Title { get; set; }

        public int? Year { get; set; } // Nullable

        public decimal? Score { get; set; } // Nullable

        public decimal? ScoreAverage { get; set; } // Nullable

        [Required]
        [StringLength(255)]
        public string Type { get; set; }

        public int? TmdbId { get; set; } // Nullable

        public int? TraktId { get; set; } // Nullable

        public int? MalId { get; set; } // Nullable
    }

    public class MovieDto
    {
        [Required]
        [StringLength(255)]
        public string ImdbId { get; set; }

        [Required]
        [StringLength(255)]
        public string Title { get; set; }

        public int? Year { get; set; } // Nullable

        public decimal? Score { get; set; } // Nullable

        public decimal? ScoreAverage { get; set; } // Nullable

        [Required]
        [StringLength(255)]
        public string Type { get; set; }

        public int? TmdbId { get; set; } // Nullable

        public int? TraktId { get; set; } // Nullable

        public int? MalId { get; set; } // Nullable
    }
}