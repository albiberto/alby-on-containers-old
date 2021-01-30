using Catalog.Models;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Infrastructure
{
    public class LuciferContext : DbContext
    {
        public LuciferContext(DbContextOptions<LuciferContext> options) : base(options)
        {
        }

        public virtual DbSet<Product> Products { get; set; }
        public virtual DbSet<Category> Categories { get; set; }
        public virtual DbSet<Attr> Attrs { get; set; }
        public virtual DbSet<AttrDesc> AttrDescs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            OnProductCreating(modelBuilder);
            OnCategoryCreating(modelBuilder);
            OnAttributeCreating(modelBuilder);
            OnDescriptionCreating(modelBuilder);
        }

        public void Upsert(object entity) => ChangeTracker.TrackGraph(entity, e => e.Entry.State = e.Entry.IsKeySet ? EntityState.Modified : EntityState.Added);

        static void OnProductCreating(ModelBuilder modelBuilder)
        {
            var productBuilder = modelBuilder.Entity<Product>();

            productBuilder
                .HasKey(product => product.Id);

            productBuilder
                .HasOne(product => product.Category)
                .WithMany(category => category.Products)
                .IsRequired(required: true)
                .OnDelete(DeleteBehavior.NoAction)
                .HasForeignKey(category => category.CategoryId);
        }

        static void OnCategoryCreating(ModelBuilder modelBuilder)
        {
            var categoryBuilder = modelBuilder.Entity<Category>();

            categoryBuilder
                .HasKey(category => category.Id);

            categoryBuilder
                .HasOne(category => category.ParentCategory)
                .WithMany()
                .IsRequired(required: false)
                .OnDelete(DeleteBehavior.NoAction)
                .HasForeignKey(category => category.ParentCategoryId);
        }

        static void OnAttributeCreating(ModelBuilder modelBuilder)
        {
            var attributeBuilder = modelBuilder.Entity<Attr>();

            attributeBuilder
                .HasKey(attribute => attribute.Id);
        }

        static void OnDescriptionCreating(ModelBuilder modelBuilder)
        {
            var attributeDescriptionBuilder = modelBuilder.Entity<AttrDesc>();

            attributeDescriptionBuilder
                .HasOne(description => description.Attribute)
                .WithMany(attribute => attribute.Descriptions)
                .IsRequired(required: true)
                .OnDelete(DeleteBehavior.NoAction)
                .HasForeignKey(description => description.AttributeId);

            attributeDescriptionBuilder
                .HasOne(description => description.Product)
                .WithMany(product => product.Descriptions)
                .IsRequired(required: true)
                .OnDelete(DeleteBehavior.NoAction)
                .HasForeignKey(description => description.ProductId);
        }
    }
}