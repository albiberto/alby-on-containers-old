using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Catalog.Models;

namespace Catalog.Infrastructure
{
    public class LuciferDbContextSeed
    {
        static readonly ICollection<Guid> _categoryIds = new[] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };
        static readonly ICollection<Guid> _attrIds = new[] { Guid.NewGuid(), Guid.NewGuid() };
        static readonly ICollection<Guid> _attrDescIds = new[] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };
        static readonly ICollection<Guid> _productIds = new[] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };

        public async Task SeedAsync(LuciferContext context)
        {
            if (!context.Categories!.Any())
            {
                await context.Categories!.AddRangeAsync(GetDefaultCategories());
                await context.SaveChangesAsync();
            }

            if (!context.Attrs!.Any())
            {
                await context.Attrs!.AddRangeAsync(GetDefaultAttrs());
                await context.SaveChangesAsync();
            }

            if (!context.Products!.Any())
            {
                await context.Products!.AddRangeAsync(GetDefaultProducts());
                await context.SaveChangesAsync();
            }

            if (!context.AttrDescs!.Any())
            {
                await context.AttrDescs!.AddRangeAsync(GetDefaultAttrDescs());
                await context.SaveChangesAsync();
            }

            await context.SaveChangesAsync();
        }

        static IEnumerable<Category> GetDefaultCategories() =>
            new List<Category>
            {
                new(_categoryIds.ElementAt(index: 0), "Frutta", "La nostra bella frutta di stagione"),
                new(_categoryIds.ElementAt(index: 1), "Mele", "Le nostre mele saporite", _categoryIds.ElementAt(index: 0)),
                new(_categoryIds.ElementAt(index: 2), "Mele Verdi", "La nostra varita' di mele verdi", _categoryIds.ElementAt(index: 1)),
                new(_categoryIds.ElementAt(index: 3), "Verdura", "La nostra verdura freschissima")
            };

        static IEnumerable<AttrAggregate> GetDefaultAttrs() =>
            new[]
            {
                new AttrAggregate(_attrIds.ElementAt(index: 0), "Descrzione"),
                new AttrAggregate(_attrIds.ElementAt(index: 1), "Le nostre ricette")
            };

        static IEnumerable<ProductAggregate> GetDefaultProducts() =>
            new[]
            {
                new ProductAggregate(_productIds.ElementAt(index: 0), "Elstat", _categoryIds.ElementAt(index: 2)),
                new ProductAggregate(_productIds.ElementAt(index: 1), "Campania", _categoryIds.ElementAt(index: 2))
            };

        static IEnumerable<AttrDesc> GetDefaultAttrDescs() =>
            new[]
            {
                new AttrDesc(_attrDescIds.ElementAt(index: 0), "Mele verdi dal sapore aspro ma dolce", _productIds.ElementAt(index: 0), _attrIds.ElementAt(index: 0)),
                new AttrDesc(_attrDescIds.ElementAt(index: 1), "Mele verdi dal sapore aspro ma dolce", _productIds.ElementAt(index: 1), _attrIds.ElementAt(index: 0)),
                new AttrDesc(_attrDescIds.ElementAt(index: 2), "Mele verdi dal sapore aspro ma dolce", _productIds.ElementAt(index: 0), _attrIds.ElementAt(index: 1)),
                new AttrDesc(_attrDescIds.ElementAt(index: 3), "Mele verdi dal sapore aspro ma dolce", _productIds.ElementAt(index: 1), _attrIds.ElementAt(index: 1))
            };
    }
}