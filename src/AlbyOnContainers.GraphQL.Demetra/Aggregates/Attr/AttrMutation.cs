using System.Threading;
using System.Threading.Tasks;
using Demetra.Extensions;
using Demetra.Infrastructure;
using Demetra.Model;
using HotChocolate;
using HotChocolate.Types;

namespace Demetra.Aggregates.Attr
{
    [ExtendObjectType(Name = "Mutation")]
    public class AttrMutations
    {
        [UseApplicationDbContext]
        public async Task<AddAttrPayload> AddAttrAsync(AddAttrInput input, [ScopedService] ApplicationDbContext context, CancellationToken cancellationToken)
        {
            var (name, description) = input;
            
            if (string.IsNullOrEmpty(name))
            {
                return new (new UserError("The name cannot be empty.", "NAME_EMPTY"));
            }

            var attr = new Model.Attr(name, description);

            await context.Attrs.AddAsync(attr, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);

            return new (attr);
        }
    }
}