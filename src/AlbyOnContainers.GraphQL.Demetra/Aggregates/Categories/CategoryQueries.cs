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

namespace Demetra.Aggregates.Categories
{
    [ExtendObjectType(Name = "Query")]
    public class CategoryQueries
    {
        [UseApplicationDbContext]
        public Task<List<Model.Category>> GetCategoriesAsync([ScopedService] ApplicationDbContext context) => context.Categories.ToListAsync();
        
        public Task<Model.Category> GetCategoryAsync(
            [ID(nameof(Model.Attr))]Guid id,
            CategoryByIdDataLoader dataLoader,
            CancellationToken cancellationToken) =>
            dataLoader.LoadAsync(id, cancellationToken);
    }
}