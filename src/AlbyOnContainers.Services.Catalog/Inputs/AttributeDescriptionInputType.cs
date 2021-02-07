using Catalog.Models;
using GraphQL.Types;

namespace Catalog.Inputs
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public sealed class AttributeDescriptionInputType : InputObjectGraphType<AttrDesc>
    {
        public AttributeDescriptionInputType()
        {
            Name = "AttributeDescriptionInput";

            Field(description => description.Id, nullable: true);

            Field(description => description.Description);

            Field(description => description.ProductId);
            Field(description => description.AttributeId);
        }
    }
}