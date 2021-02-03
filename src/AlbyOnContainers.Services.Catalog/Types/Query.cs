using System;
using Catalog.Repository;
using GraphQL;
using GraphQL.Types;
using GraphQL.Utilities;

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
        }

        void ProductQueries()
        {
            FieldAsync<ProductType>(
                "product",
                "A single product of the company.",
                new QueryArguments(new QueryArgument<StringGraphType> {Name = "Id", Description = "Product Id"}),
                async context =>
                {
                    var id = context.GetArgument<Guid>("id");

                    return await _provider
                        .GetRequiredService<ProductRepository>()
                        .FindAsync(id);
                });

            FieldAsync<ListGraphType<ProductType>>(
                "products",
                "The list of the company products",
                resolve: async context =>
                    await _provider
                        .GetRequiredService<ProductRepository>()
                        .GetAllAsync());
        }

        void CategoryQueries()
        {
            FieldAsync<CategoryType>(
                "category",
                "A single category of the company.",
                new QueryArguments(new QueryArgument<StringGraphType> {Name = "Id", Description = "Category Id"}),
                async context =>
                {
                    var id = context.GetArgument<Guid>("id");

                    return await _provider
                        .GetRequiredService<CategoryRepository>()
                        .FindAsync(id);
                });

            FieldAsync<ListGraphType<CategoryType>>(
                "categories",
                "The list of the categories",
                resolve: async context =>
                    await _provider
                        .GetRequiredService<CategoryRepository>()
                        .GetAllAsync());
        }

        void AttributeQueries()
        {
            FieldAsync<AttributeType>(
                "attribute",
                "A single attribute.",
                new QueryArguments(new QueryArgument<StringGraphType> {Name = "Id", Description = "Attribute Id"}),
                async context =>
                {
                    var id = context.GetArgument<Guid>("id");

                    return await _provider
                        .GetRequiredService<AttrRepository>()
                        .FindAsync(id);
                });

            FieldAsync<ListGraphType<AttributeType>>(
                "attributes",
                "The list of attributes", resolve: async context =>
                    await _provider
                        .GetRequiredService<AttrRepository>()
                        .GetAllAsync());

            FieldAsync<AttributeDescriptionType>(
                "description",
                "A single attribute description of a product.",
                new QueryArguments(
                    new QueryArgument<StringGraphType> {Name = "Id", Description = "Attribute Description Id"},
                    new QueryArgument<StringGraphType> {Name = "productId", Description = "product Id"}),
                async context =>
                {
                    var id = context.GetArgument<Guid>("id");

                    return await _provider
                        .GetRequiredService<AttrRepository>()
                        .FindDescriptionAsync(id);
                });

            FieldAsync<ListGraphType<AttributeDescriptionType>>(
                "descriptions",
                "The list of attribute descriptions",
                resolve: async context =>
                    await _provider
                        .GetRequiredService<AttrRepository>()
                        .GetAllDescriptionsAsync());
        }
    }
}