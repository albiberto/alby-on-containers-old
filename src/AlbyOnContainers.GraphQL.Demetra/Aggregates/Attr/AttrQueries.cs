using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Demetra.DataLoader;
using Demetra.Extensions;
using Demetra.Infrastructure;
using HotChocolate;
using HotChocolate.Types;
using HotChocolate.Types.Relay;
using Microsoft.EntityFrameworkCore;

namespace Demetra.Aggregates.Attr
{
    [ExtendObjectType(Name = "Query")]
    public class AttrQueries
    {
        [UseApplicationDbContext]
        public Task<List<Model.Attr>> GetAttrAsync([ScopedService] ApplicationDbContext context) => context.Attrs.ToListAsync();
        
        public Task<Model.Attr> GetAttrAsync(
            [ID(nameof(Model.Attr))]Guid id,
            AttrByIdDataLoader dataLoader,
            CancellationToken cancellationToken) =>
            dataLoader.LoadAsync(id, cancellationToken);
    }
}