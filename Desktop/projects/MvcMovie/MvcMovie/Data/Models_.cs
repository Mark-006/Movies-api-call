using Microsoft.EntityFrameworkCore;

namespace ApplicationDbContext_MvcMovie
{
    public class Models_ : DbContext
    {
        public Models_(DbContextOptions<Models_> options)
            : base(options)
        {
        }

        public DbSet<MvcMovie.Models.Movie> Movie { get; set; } = default!;
        public object Movies { get; internal set; }
    }
}
