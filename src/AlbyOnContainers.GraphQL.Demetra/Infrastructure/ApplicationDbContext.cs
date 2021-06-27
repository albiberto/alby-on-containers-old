using Demetra.Model;
using Microsoft.EntityFrameworkCore;

namespace Demetra.Infrastructure
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var product = modelBuilder.Entity<Product>();
            product.HasKey(p => p.Id);
            product.HasIndex(p => p.Name).IsUnique();
            product.Property(p => p.Description);

            product
                .HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId)
                .IsRequired()
                .OnDelete(DeleteBehavior.NoAction);

            product
                .HasMany(p => p.Descrs)
                .WithOne(d => d.Product)
                .HasForeignKey(d => d.ProductId)
                .IsRequired()
                .OnDelete(DeleteBehavior.NoAction);

            var category = modelBuilder.Entity<Category>();
            category.HasKey(c => c.Id);
            category.HasIndex(c => c.Name).IsUnique();
            category.Property(c => c.Description);
            
            category
                .HasOne(c => c.Parent)
                .WithMany()
                .IsRequired(false)
                .OnDelete(DeleteBehavior.NoAction)
                .HasForeignKey(c => c.ParentId);

            var attr = modelBuilder.Entity<Attr>();
            attr.HasKey(a => a.Id);
            attr.HasIndex(a => a.Name).IsUnique();
            attr.Property(a => a.Description);
            
            attr
                .HasMany(p => p.Descrs)
                .WithOne(a => a.Attr)
                .HasForeignKey(a => a.AttrId)
                .IsRequired()
                .OnDelete(DeleteBehavior.NoAction);

            var attrDescrs = modelBuilder.Entity<AttrDescr>();
            attrDescrs.HasKey(d => d.Id);
            attrDescrs.HasIndex(d => d.Name).IsUnique();
            attrDescrs.Property(d => d.Description);
            attrDescrs.Property(d => d.ProductId);
            attrDescrs.Property(d => d.AttrId);
        }

        public DbSet<Product> Products { get; set; } = default!;
        public DbSet<Attr> Attrs { get; set; } = default!;
        public DbSet<AttrDescr> AttrDescrs { get; set; } = default!;
        public DbSet<Category> Categories { get; set; } = default!;
    }
}