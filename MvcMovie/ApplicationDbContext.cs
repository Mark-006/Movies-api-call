using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MvcMovie.Models;

namespace MvcMovie.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<AccountUser> AccountUsers { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Customize ASP.NET Identity model to use custom table names
            builder.Entity<AccountUser>(entity =>
            {
                entity.ToTable(name: "AccountUsers"); // Set custom table name

                // Set primary key
                entity.HasKey(e => e.UserId);

                // Configure the primary key to be auto-generated
                entity.Property(e => e.UserId)
                    .ValueGeneratedOnAdd();

                // Configure other properties if needed
            });

            // Customize other entities if necessary
        }
    }
}
