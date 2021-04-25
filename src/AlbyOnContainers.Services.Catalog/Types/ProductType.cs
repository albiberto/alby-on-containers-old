using System;
using System.Collections.Generic;
using System.Linq;
using Catalog.Models;
using Catalog.Repository;
using GraphQL.DataLoader;
using GraphQL.Types;
using GraphQL.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace Catalog.Types
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public sealed class ProductType : ObjectGraphType<ProductAggregate>
    {
        public ProductType(IServiceProvider provider, IDataLoaderContextAccessor dataLoader)
        {
            Name = "Product";
            Description = "A products sold by the company.";

            Field(d => d.Id, nullable: true).Description("The id of the product.");
            Field(d => d.Name).Description("The name of the product.");
            Field(d => d.CategoryId).Description("The id og the product category.");

            Field<CategoryType, Category>()
                .Name("category")
                .Description("The product category.")
                .ResolveAsync(context =>
                {
                    var loader = dataLoader.Context.GetOrAddBatchLoader<Guid, Category>("GetCategoryByIds",
                        async ids => await provider.GetRequiredService<CategoryRepository>().Filter(ids));

                    return loader.LoadAsync(context.Source.CategoryId);
                });

            Field<ListGraphType<AttributeDescriptionType>, IEnumerable<AttrDesc>>()
                .Name("descriptions")
                .Description("The product attributes.")
                .ResolveAsync(context =>
                {
                    var loader = dataLoader.Context.GetOrAddCollectionBatchLoader<Guid, AttrDesc>("GetAttributesByProductId", async ids =>
                        (await provider.GetRequiredService<AttrRepository>().GetDescriptionsAsync(ids))
                        .ToLookup(e => e.ProductId));

                    return loader.LoadAsync(context.Source.Id!.Value);
                });
        }
    }
}