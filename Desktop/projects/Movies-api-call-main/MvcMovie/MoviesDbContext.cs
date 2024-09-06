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

        public DbSet<Movie> Movies { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Set the primary key for the Movie entity
            modelBuilder.Entity<Movie>()
                .HasKey(m => m.ImdbId);

            // Configure any additional settings, such as relationships or constraints
            modelBuilder.Entity<Movie>()
                .Property(m => m.Title)
                .IsRequired()
                .HasMaxLength(255);

            modelBuilder.Entity<Movie>()
                .Property(m => m.Year)
                .IsRequired();

            modelBuilder.Entity<Movie>()
                .Property(m => m.Score)
                .HasPrecision(3, 1); // Adjust precision as needed

            // Additional configuration can be added here

            base.OnModelCreating(modelBuilder);
        }
    }
}
