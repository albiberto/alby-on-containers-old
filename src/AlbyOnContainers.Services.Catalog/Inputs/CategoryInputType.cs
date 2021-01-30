using Catalog.Models;
using GraphQL.Types;

namespace Catalog.Inputs
{
    public sealed class CategoryInputType : InputObjectGraphType<Category>
    {
        public CategoryInputType()
        {
            Name = "CategoryInput";

            Field(x => x.Id, true);
            Field(x => x.Name, true);
            Field(x => x.Description, true);
        }
    }
}