using Catalog.Models;
using GraphQL.Types;

namespace Catalog.Inputs
{
    public sealed class AttributeDescriptionInputType : InputObjectGraphType<AttrDesc>
    {
        public AttributeDescriptionInputType()
        {
            Name = "AttributeDescriptionInput";

            Field(description => description.Id);

            Field(description => description.Description);

            Field(description => description.ProductId);
            Field(description => description.AttributeId);
        }
    }
}