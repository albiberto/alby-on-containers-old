using System.Threading;
using System.Threading.Tasks;
using Demetra.Extensions;
using Demetra.Infrastructure;
using Demetra.Model;
using HotChocolate;
using HotChocolate.Types;

namespace Demetra.Aggregates.Categories
{
    [ExtendObjectType(Name = "Mutation")]
    public class CategoryMutation
    {
        [UseApplicationDbContext]
        public async Task<AddCategoryPayload> AddCategoryAsync (AddCategoryInput input, [ScopedService] ApplicationDbContext context, CancellationToken cancellationToken)
        {
            var (name, description, parentId) = input;
            
            if (string.IsNullOrEmpty(name))
            {
                return new (new UserError("The name cannot be empty.", "NAME_EMPTY"));
            }

            var attr = new Category(name, description, parentId);

            await context.Categories.AddAsync(attr, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);

            return new (attr);
        }
    }
}