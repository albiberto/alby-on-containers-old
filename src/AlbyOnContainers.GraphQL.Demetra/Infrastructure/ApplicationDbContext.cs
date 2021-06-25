using Demetra.Model;
using Microsoft.EntityFrameworkCore;

namespace Demetra.Infrastructure
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Product> Products { get; set; } = default!;
    }
}