using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Catalog.Models;
using Microsoft.Extensions.Logging;

namespace Catalog.Infrastructure
{
    public class LuciferDbContextSeed
    {
        static readonly ICollection<Guid> CategoryIds = new[] {Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()};
        static readonly ICollection<Guid> AttrIds = new[] {Guid.NewGuid(), Guid.NewGuid()};
        static readonly ICollection<Guid> AttrDescIds = new[] {Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()};
        static readonly ICollection<Guid> ProductIds = new[] {Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()};

        public async Task SeedAsync(LuciferContext context, ILogger<LuciferDbContextSeed> logger, int retry = 10)
        {
            try
            {
                if (!context.Categories.Any()) await context.Categories.AddRangeAsync(GetDefaultCategories());
                if (!context.Attrs.Any()) await context.Attrs.AddRangeAsync(GetDefaultAttrs());
                if (!context.Products.Any()) await context.Products.AddRangeAsync(GetDefaultProducts());
                if (!context.AttrDescs.Any()) await context.AttrDescs.AddRangeAsync(GetDefaultAttrDescs());

                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                if (retry < 10)
                {
                    retry++;

                    logger.LogError(ex, "EXCEPTION ERROR while migrating {DbContextName}", nameof(LuciferContext));

                    await SeedAsync(context, logger, retry);
                }
            }
        }

        static IEnumerable<Category> GetDefaultCategories() =>
            new List<Category>
            {
                new()
                {
                    Id = CategoryIds.ElementAt(0),
                    Name = "Frutta",
                    Description = "La nostra bella frutta di stagione",
                    ParentCategoryId = default
                },
                new()
                {
                    Id = CategoryIds.ElementAt(1),
                    Name = "Mele",
                    Description = "Le nostre mele saporite",
                    ParentCategoryId = CategoryIds.ElementAt(0)
                },
                new()
                {
                    Id = CategoryIds.ElementAt(2),
                    Name = "Mele Verdi",
                    Description = "La nostra varita' di mele verdi",
                    ParentCategoryId = CategoryIds.ElementAt(1)
                },
                new()
                {
                    Id = CategoryIds.ElementAt(3),
                    Name = "Verdura",
                    Description = "La nostra verdura freschissima",
                    ParentCategoryId = default
                }
            };

        static IEnumerable<Attr> GetDefaultAttrs() =>
            new[]
            {
                new Attr
                {
                    Id = AttrIds.ElementAt(0),
                    Name = "Descrizione"
                },
                new Attr
                {
                    Id = AttrIds.ElementAt(1),
                    Name = "Le nostre ricette"
                }
            };

        static IEnumerable<Product> GetDefaultProducts() =>
            new[]
            {
                new Product
                {
                    Id = ProductIds.ElementAt(0),
                    Name = "Elstat",
                    CategoryId = CategoryIds.ElementAt(2)
                },
                new Product
                {
                    Id = ProductIds.ElementAt(1),
                    Name = "Campanina",
                    CategoryId = CategoryIds.ElementAt(2)
                }
            };

        static IEnumerable<AttrDesc> GetDefaultAttrDescs() =>
            new[]
            {
                new AttrDesc
                {
                    Id = AttrDescIds.ElementAt(0),
                    AttributeId = AttrIds.ElementAt(0),
                    ProductId = ProductIds.ElementAt(0),
                    Description = "Mele verdi dal sapore aspro ma dolce"
                },
                new AttrDesc
                {
                    Id = AttrDescIds.ElementAt(1),
                    AttributeId = AttrIds.ElementAt(0),
                    ProductId = ProductIds.ElementAt(1),
                    Description = "Mele gialle dal sapore dolce ma aspro"
                },
                new AttrDesc
                {
                    Id = AttrDescIds.ElementAt(2),
                    AttributeId = AttrIds.ElementAt(1),
                    ProductId = ProductIds.ElementAt(0),
                    Description = "Cotto e mangiato"
                },
                new AttrDesc
                {
                    Id = AttrDescIds.ElementAt(3),
                    AttributeId = AttrIds.ElementAt(1),
                    ProductId = ProductIds.ElementAt(1),
                    Description = "Crudo e mangiato"
                }
            };
    }
}