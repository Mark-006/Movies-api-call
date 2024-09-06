using Microsoft.EntityFrameworkCore;
using MvcMovie.Models;

namespace MvcMovie.Data
{
    public class MoviesDbContext : DbContext
    {
        public MoviesDbContext(DbContextOptions<MoviesDbContext> options)
            : base(options)
        {
        }

        // DbSet for Movies
        public DbSet<Movie> Movies { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure the primary key for the Movie entity
            modelBuilder.Entity<Movie>()
                .HasKey(m => m.ImdbId);

            // Optionally, you can configure additional model properties here
            // For example:
            // modelBuilder.Entity<Movie>()
            //     .Property(m => m.Title)
            //     .HasMaxLength(255)
            //     .IsRequired();

            base.OnModelCreating(modelBuilder);
        }
    }
}
