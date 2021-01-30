using System;
using Catalog.Infrastructure;
using GraphQL;
using GraphQL.Types;
using GraphQL.Utilities;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Types
{
    public sealed class Query : ObjectGraphType<object>
    {
        readonly IServiceProvider _provider;

        public Query(IServiceProvider provider)
        {
            Name = "Query";

            _provider = provider;

            ProductQueries();
            CategoryQueries();
            AttributeQueries();
            AttributeDescriptionQueries();
        }

        void ProductQueries()
        {
            FieldAsync<ProductType>(
                "product",
                "A single product of the company.",
                new QueryArguments(new QueryArgument<StringGraphType> { Name = "Id", Description = "Product Id" }),
                async context =>
                {
                    var id = context.GetArgument<Guid>("id");

                    return await _provider.GetRequiredService<LuciferContext>()
                        .Products
                        .FirstOrDefaultAsync(product => product.Id == id);
                });

            FieldAsync<ListGraphType<ProductType>>(
                "products",
                "The list of the company products",
                resolve: async context =>
                    await _provider.GetRequiredService<LuciferContext>()
                        .Products
                        .ToListAsync());
        }

        void CategoryQueries()
        {
            FieldAsync<CategoryType>(
                "category",
                "A single category of the company.",
                new QueryArguments(new QueryArgument<StringGraphType> { Name = "Id", Description = "Category Id" }),
                async context =>
                {
                    var id = context.GetArgument<Guid>("id");

                    return await _provider.GetRequiredService<LuciferContext>()
                        .Categories
                        .FirstOrDefaultAsync(category => category.Id == id);
                });

            FieldAsync<ListGraphType<CategoryType>>(
                "categories",
                "The list of the categories",
                resolve: async context =>
                    await _provider.GetRequiredService<LuciferContext>()
                        .Categories
                        .ToListAsync());
        }

        void AttributeQueries()
        {
            FieldAsync<AttributeType>(
                "attribute",
                "A single attribute.",
                new QueryArguments(new QueryArgument<StringGraphType> { Name = "Id", Description = "Attribute Id" }),
                async context =>
                {
                    var id = context.GetArgument<Guid>("id");

                    return await _provider.GetRequiredService<LuciferContext>()
                        .Attrs
                        .FirstOrDefaultAsync(attribute => attribute.Id == id);
                });

            FieldAsync<ListGraphType<AttributeType>>(
                "attributes",
                "The list of attributes", resolve: async context =>
                    await _provider.GetRequiredService<LuciferContext>()
                        .Attrs
                        .ToListAsync());
        }

        void AttributeDescriptionQueries()
        {
            FieldAsync<AttributeDescriptionType>(
                "description",
                "A single attribute description of a product.",
                new QueryArguments(
                    new QueryArgument<StringGraphType> { Name = "Id", Description = "Attribute Description Id" },
                    new QueryArgument<StringGraphType> { Name = "productId", Description = "product Id" }),
                async context =>
                {
                    var id = context.GetArgument<Guid>("id");

                    return await _provider.GetRequiredService<LuciferContext>()
                        .AttrDescs
                        .FirstOrDefaultAsync(description => description.Id == id);
                });

            FieldAsync<ListGraphType<AttributeDescriptionType>>(
                "descriptions",
                "The list of attribute descriptions",
                resolve: async context =>
                    await _provider.GetRequiredService<LuciferContext>()
                        .AttrDescs
                        .ToListAsync());
        }
    }
}