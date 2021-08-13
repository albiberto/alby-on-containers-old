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

namespace Demetra.Aggregates.AttrDescr
{
    [ExtendObjectType(Name = "Query")]
    public class AttrDescrQueries
    {
        [UseApplicationDbContext]
        public Task<List<Model.AttrDescr>> GetAttrDescrsAsync([ScopedService] ApplicationDbContext context) => context.AttrDescrs.ToListAsync();
        
        public Task<Model.AttrDescr> GetAttrDescrAsync(
            [ID(nameof(Model.Attr))]Guid id,
            AttrDescrByIdDataLoader dataLoader,
            CancellationToken cancellationToken) =>
            dataLoader.LoadAsync(id, cancellationToken);
    }
}