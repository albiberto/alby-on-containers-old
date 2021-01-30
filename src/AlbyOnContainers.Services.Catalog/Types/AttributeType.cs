using Catalog.Models;
using GraphQL.Types;

namespace Catalog.Types
{
    public sealed class AttributeType : ObjectGraphType<Attr>
    {
        public AttributeType()
        {
            Name = "Attribute";
            Description = "A macro attribute associated to the product.";

            Field(d => d.Id).Description("The id of attribute.");
            Field(d => d.Name).Description("The name of attribute.");
        }
    }
}