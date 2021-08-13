using System;
using System.Threading;
using System.Threading.Tasks;
using Demetra.Aggregates.Attr;
using Demetra.Extensions;
using Demetra.Infrastructure;
using Demetra.Model;
using HotChocolate;
using HotChocolate.Types;

namespace Demetra.Aggregates.AttrDescr
{
    [ExtendObjectType(Name = "Mutation")]
    public class AttrDescrMutation
    {
        [UseApplicationDbContext]
        public async Task<AddAttrDescrPayload> AddAttrDescrAsync(AddAttrDescrInput input, [ScopedService] ApplicationDbContext context, CancellationToken cancellationToken)
        {
            var (name, description, attrId, productId) = input;
            
            if (string.IsNullOrEmpty(name))
            {
                return new (new UserError("The name cannot be empty.", "NAME_EMPTY"));
            }
            
            if (attrId == Guid.Empty)
            {
                return new (new UserError("Attr id cannot be empty.", "ATTR_EMPTY"));
            }

            var attr = new Model.AttrDescr(name, description, productId, attrId);

            await context.AttrDescrs.AddAsync(attr, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);

            return new (attr);
        }
    }
}