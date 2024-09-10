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
        public DbSet<Role> Roles { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Movie>(entity =>
            {
                entity.Property(e => e.ImdbId)
                    .HasMaxLength(255)
                    .HasColumnType("nvarchar(255)");

                entity.Property(e => e.Title)
                    .IsRequired()
                    .HasMaxLength(255)
                    .HasColumnType("nvarchar(255)");

                entity.Property(e => e.Type)
                    .IsRequired()
                    .HasMaxLength(255)
                    .HasColumnType("nvarchar(255)");

                entity.Property(e => e.Year)
                    .HasColumnType("int")
                    .HasDefaultValue(0);

                entity.Property(e => e.Score)
                    .HasColumnType("decimal(18, 2)")
                    .HasDefaultValue(0.0m);

                entity.Property(e => e.ScoreAverage)
                    .HasColumnType("decimal(18, 2)")
                    .HasDefaultValue(0.0m);

                entity.Property(e => e.TmdbId)
                    .HasColumnType("int")
                    .HasDefaultValue(0);

                entity.Property(e => e.TraktId)
                    .HasColumnType("int")
                    .HasDefaultValue(0);

                entity.Property(e => e.MalId)
                    .HasColumnType("int")
                    .HasDefaultValue(0);

                entity.HasKey(e => e.ImdbId);

                entity.ToTable("Movies");
            });
        }
    }

}
