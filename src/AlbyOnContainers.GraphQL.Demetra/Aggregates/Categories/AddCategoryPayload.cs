using Demetra.Common;
using Demetra.Model;

namespace Demetra.Aggregates.Categories
{
    public class AddCategoryPayload : Payload
    {
        public AddCategoryPayload(Category category)
        {
            Category = category;
        }

        public AddCategoryPayload(UserError error) : base(new[] {error})
        {
        }

        public Category? Category { get; }
    }
}