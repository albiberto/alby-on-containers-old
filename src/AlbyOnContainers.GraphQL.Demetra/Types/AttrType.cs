using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Demetra.DataLoader;
using Demetra.Extensions;
using Demetra.Infrastructure;
using Demetra.Model;
using GreenDonut;
using HotChocolate;
using HotChocolate.Resolvers;
using HotChocolate.Types;
using Microsoft.EntityFrameworkCore;

namespace Demetra.Types
{
    public class AttrType: ObjectType<Attr>
    {
        protected override void Configure(IObjectTypeDescriptor<Attr> descriptor)
        {
            descriptor
                .ImplementsNode()
                .IdField(t => t.Id)
                .ResolveNode((ctx, id) => ctx.DataLoader<AttrByIdDataLoader>().LoadAsync(id, ctx.RequestAborted));

            descriptor
                .Field(t => t.Descrs)
                .ResolveWith<AttrResolvers>(t => t.GetSessionsAsync(default!, default!, default!, default))
                .UseDbContext<ApplicationDbContext>()
                .Name("descrs");
        }

        class AttrResolvers
        {
            public async Task<IEnumerable<AttrDescr>> GetSessionsAsync(EntityBase attendee, [ScopedService] ApplicationDbContext dbContext,
                AttrDescrByIdDataLoader attrDescrByIdDataLoader, CancellationToken cancellationToken)
            {
                var attrDescrIds = await dbContext.AttrDescrs
                    .Where(a => a.Id == attendee.Id)
                    .Select(a => a.Id)
                    .ToArrayAsync(cancellationToken: cancellationToken);

                return await attrDescrByIdDataLoader.LoadAsync(attrDescrIds, cancellationToken);
            }
        }
    }
}