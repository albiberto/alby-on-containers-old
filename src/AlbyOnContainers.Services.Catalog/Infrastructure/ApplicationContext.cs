using Catalog.Models;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Infrastructure
{
    public class ApplicationContext : DbContext, IUnitOfWork
    {
        public ApplicationContext(DbContextOptions<ApplicationContext> options) : base(options)
        {
        }

        public virtual DbSet<ProductAggregate>? Products { get; set; }
        public virtual DbSet<Category>? Categories { get; set; }
        public virtual DbSet<AttrAggregate>? Attrs { get; set; }
        public virtual DbSet<AttrDesc>? AttrDescs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            OnProductCreating(modelBuilder);
            OnCategoryCreating(modelBuilder);
            OnAttributeCreating(modelBuilder);
            OnDescriptionCreating(modelBuilder);
        }

        static void OnProductCreating(ModelBuilder modelBuilder)
        {
            var builder = modelBuilder.Entity<ProductAggregate>();

            builder.HasKey(e => e.Id);

            builder.Property(p => p.Name).IsRequired();

            builder
                .HasOne(product => product.Category)
                .WithMany(category => category!.Products)
                .IsRequired()
                .OnDelete(DeleteBehavior.NoAction)
                .HasForeignKey(category => category.CategoryId);
        }

        static void OnCategoryCreating(ModelBuilder modelBuilder)
        {
            var builder = modelBuilder.Entity<Category>();

            builder.HasKey(category => category.Id);

            builder.Property(p => p.Name).IsRequired();
            builder.Property(p => p.Description).IsRequired();

            builder
                .HasOne(category => category.ParentCategory)
                .WithMany()
                .IsRequired(required: false)
                .OnDelete(DeleteBehavior.NoAction)
                .HasForeignKey(category => category.ParentCategoryId);
        }

        static void OnAttributeCreating(ModelBuilder modelBuilder)
        {
            var builder = modelBuilder.Entity<AttrAggregate>();

            builder.HasKey(attribute => attribute.Id);

            builder.Property(p => p.Name);
        }

        static void OnDescriptionCreating(ModelBuilder modelBuilder)
        {
            var builder = modelBuilder.Entity<AttrDesc>();

            builder.HasKey(attribute => attribute.Id);

            builder.Property(p => p.Description);

            builder
                .HasOne(description => description.Attribute)
                .WithMany(attribute => attribute!.Descriptions)
                .IsRequired()
                .OnDelete(DeleteBehavior.NoAction)
                .HasForeignKey(description => description.AttributeId);

            builder
                .HasOne(description => description.Product)
                .WithMany(product => product!.Descriptions)
                .IsRequired()
                .OnDelete(DeleteBehavior.NoAction)
                .HasForeignKey(description => description.ProductId);
        }
    }
}