using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

namespace MvcMovie.Models
{
    public class RolesDbContext : DbContext
    {
        public RolesDbContext(DbContextOptions<RolesDbContext> options)
            : base(options)
        {
        }

        public DbSet<Role> Roles { get; set; }
        public DbSet<AccountUser> AccountUsers { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);


            // Customize ASP.NET Identity model to use custom table names

            //builder.Entity<Role>()
            //.HasKey(r => r.Id);
            builder.Entity<Role>(entity =>
            {
                entity.ToTable(name: "Roles"); // Set custom table name

                // Set primary key
                entity.HasKey(r => r.Id);

                // Configure the primary key to be auto-generated
                entity.Property(r => r.Id)
                    .ValueGeneratedOnAdd();

                // Configure other properties if needed
            });

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
