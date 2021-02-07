using Catalog.Models;
using GraphQL.Types;

namespace Catalog.Types
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public sealed class AttributeType : ObjectGraphType<AttrAggregate>
    {
        public AttributeType()
        {
            Name = "Attribute";
            Description = "A macro attribute associated to the product.";

            Field(d => d.Id, nullable: true).Description("The id of attribute.");
            Field(d => d.Name).Description("The name of attribute.");
        }
    }
}