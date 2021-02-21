using System;
using Catalog.Inputs;
using Catalog.Models;
using Catalog.Repository;
using GraphQL;
using GraphQL.Types;
using GraphQL.Utilities;

namespace Catalog.Types
{
    public class Mutation : ObjectGraphType<object>
    {
        readonly IServiceProvider _provider;

        public Mutation(IServiceProvider provider)
        {
            Name = "Mutation";

            _provider = provider;

            ProductMutations();
            CategoryMutations();
            AttributeMutations();
        }

        void ProductMutations()
        {
            const string name = "product";

            FieldAsync<ProductType>(
                "createProduct",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<ProductInputType>> { Name = name }
                ),
                resolve: async context =>
                {
                    var product = context.GetArgument<ProductAggregate>(name);
                    var repository = _provider.GetRequiredService<ProductRepository>();

                    await repository.AddAsync(product);
                    return await repository.UnitOfWork.SaveChangesAsync();
                });

            FieldAsync<ProductType>(
                "updateProduct",
                arguments: new QueryArguments(new QueryArgument<NonNullGraphType<ProductInputType>> { Name = name }),
                resolve: async context =>
                {
                    var product = context.GetArgument<ProductAggregate>(name);
                    var productRepository = _provider.GetRequiredService<ProductRepository>();

                    await productRepository.UpdateAsync(product);
                    return await productRepository.UnitOfWork.SaveChangesAsync();
                });
        }

        void CategoryMutations()
        {
            const string name = "category";

            FieldAsync<CategoryType>(
                "createCategory",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<CategoryInputType>> { Name = name }
                ),
                resolve: async context =>
                {
                    var category = context.GetArgument<Category>(name);
                    var repository = _provider.GetRequiredService<CategoryRepository>();

                    await repository.AddAsync(category);
                    return await repository.UnitOfWork.SaveChangesAsync();
                });

            FieldAsync<CategoryType>(
                "updateCategory",
                arguments: new QueryArguments(new QueryArgument<NonNullGraphType<CategoryInputType>> { Name = name }
                ),
                resolve: async context =>
                {
                    var category = context.GetArgument<Category>(name);
                    var repository = _provider.GetRequiredService<CategoryRepository>();

                    await repository.UpdateAsync(category);
                    return await repository.UnitOfWork.SaveChangesAsync();
                });
        }

        void AttributeMutations()
        {
            const string name = "attribute";

            FieldAsync<AttributeType>(
                "createAttribute",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<AttributeInputType>> { Name = name }
                ),
                resolve: async context =>
                {
                    var attribute = context.GetArgument<AttrAggregate>(name);
                    var repository = _provider.GetRequiredService<AttrRepository>();

                    await repository.AddAsync(attribute);
                    return await repository.UnitOfWork.SaveChangesAsync();
                });

            FieldAsync<AttributeType>(
                "updateAttribute",
                arguments: new QueryArguments(new QueryArgument<NonNullGraphType<AttributeInputType>> { Name = name }
                ),
                resolve: async context =>
                {
                    var attribute = context.GetArgument<AttrAggregate>(name);
                    var repository = _provider.GetRequiredService<AttrRepository>();

                    await repository.UpdateAsync(attribute);
                    return await repository.UnitOfWork.SaveChangesAsync();
                });
        }
    }
}