using System;
using Catalog.Models;
using GraphQL.DataLoader;
using GraphQL.Types;

namespace Catalog.Types
{
    public sealed class CategoryType : ObjectGraphType<Category>
    {
        public CategoryType(IServiceProvider provider, IDataLoaderContextAccessor dataLoader)
        {
            Name = "Category";
            Description = "The Product Category.";

            Field(d => d.Id, nullable:true).Description("The Category Id.");
            Field(d => d.Name).Description("The Category Name.");
            Field(d => d.Description).Description("The Category Description.");
        }
    }
}