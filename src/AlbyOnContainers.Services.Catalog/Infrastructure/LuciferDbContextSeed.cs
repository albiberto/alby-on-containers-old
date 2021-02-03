using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Catalog.Models;

namespace Catalog.Infrastructure
{
    public class LuciferDbContextSeed
    {
        static readonly ICollection<Guid> CategoryIds = new[] {Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()};
        static readonly ICollection<Guid> AttrIds = new[] {Guid.NewGuid(), Guid.NewGuid()};
        static readonly ICollection<Guid> AttrDescIds = new[] {Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()};
        static readonly ICollection<Guid> ProductIds = new[] {Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()};

        public async Task SeedAsync(LuciferContext context)
        {
            if (!context.Categories!.Any())
            {
                await context.Categories.AddRangeAsync(GetDefaultCategories());
                await context.SaveChangesAsync();
            }

            if (!context.Attrs!.Any())
            {
                await context.Attrs.AddRangeAsync(GetDefaultAttrs());
                await context.SaveChangesAsync();
            }

            if (!context.Products!.Any())
            {
                await context.Products.AddRangeAsync(GetDefaultProducts());
                await context.SaveChangesAsync();
            }


            if (!context.AttrDescs!.Any())
            {
                await context.AttrDescs.AddRangeAsync(GetDefaultAttrDescs());
                await context.SaveChangesAsync();
            }

            await context.SaveChangesAsync();
        }

        static IEnumerable<Category> GetDefaultCategories() =>
            new List<Category>
            {
                new(CategoryIds.ElementAt(0), "Frutta", "La nostra bella frutta di stagione"),
                new(CategoryIds.ElementAt(1), "Mele", "Le nostre mele saporite", CategoryIds.ElementAt(0)),
                new(CategoryIds.ElementAt(2), "Mele Verdi", "La nostra varita' di mele verdi", CategoryIds.ElementAt(1)),
                new(CategoryIds.ElementAt(3), "Verdura", "La nostra verdura freschissima")
            };

        static IEnumerable<AttrAggregate> GetDefaultAttrs() =>
            new[]
            {
                new AttrAggregate(AttrIds.ElementAt(0), "Descrzione"),
                new AttrAggregate(AttrIds.ElementAt(1), "Le nostre ricette")
            };

        static IEnumerable<ProductAggregate> GetDefaultProducts() =>
            new[]
            {
                new ProductAggregate(ProductIds.ElementAt(0), "Elstat", CategoryIds.ElementAt(2)),
                new ProductAggregate(ProductIds.ElementAt(1), "Campania", CategoryIds.ElementAt(2))
            };

        static IEnumerable<AttrDesc> GetDefaultAttrDescs() =>
            new[]
            {
                new AttrDesc(AttrDescIds.ElementAt(0), "Mele verdi dal sapore aspro ma dolce", ProductIds.ElementAt(0), AttrIds.ElementAt(0)),
                new AttrDesc(AttrDescIds.ElementAt(1), "Mele verdi dal sapore aspro ma dolce", ProductIds.ElementAt(1), AttrIds.ElementAt(0)),
                new AttrDesc(AttrDescIds.ElementAt(2), "Mele verdi dal sapore aspro ma dolce", ProductIds.ElementAt(0), AttrIds.ElementAt(1)),
                new AttrDesc(AttrDescIds.ElementAt(3), "Mele verdi dal sapore aspro ma dolce", ProductIds.ElementAt(1), AttrIds.ElementAt(1))
            };
    }
}