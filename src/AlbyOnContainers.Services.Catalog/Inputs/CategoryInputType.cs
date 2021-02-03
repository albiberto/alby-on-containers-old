using Catalog.Models;
using GraphQL.Types;

namespace Catalog.Inputs
{
    public sealed class CategoryInputType : InputObjectGraphType<Category>
    {
        public CategoryInputType()
        {
            Name = "CategoryInput";

            Field(x => x.Id, nullable: true);
            Field(x => x.Name, nullable: true);
            Field(x => x.Description, nullable: true);
        }
    }
}