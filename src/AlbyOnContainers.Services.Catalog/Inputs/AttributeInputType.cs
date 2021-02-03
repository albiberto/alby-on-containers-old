using Catalog.Models;
using GraphQL.Types;

namespace Catalog.Inputs
{
    public sealed class AttributeInputType : InputObjectGraphType<AttrAggregate>
    {
        public AttributeInputType()
        {
            Name = "AttributeInput";

            Field(x => x.Id, nullable:true);
            Field(x => x.Name);
        }
    }
}