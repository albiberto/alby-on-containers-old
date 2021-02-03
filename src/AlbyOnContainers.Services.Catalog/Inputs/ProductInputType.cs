using Catalog.Models;
using GraphQL.Types;

namespace Catalog.Inputs
{
    public sealed class ProductInputType : InputObjectGraphType<ProductAggregate>
    {
        public ProductInputType()
        {
            Name = "ProductInput";

            Field(product => product.Id, true);
            Field(product => product.Name);
            Field(product => product.CategoryId);
            Field<ListGraphType<AttributeDescriptionInputType>>("descriptions");
        }
    }
}