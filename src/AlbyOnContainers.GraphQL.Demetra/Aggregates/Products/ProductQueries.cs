using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Demetra.DataLoader;
using Demetra.Extensions;
using Demetra.Infrastructure;
using Demetra.Model;
using HotChocolate;
using HotChocolate.Types;
using HotChocolate.Types.Relay;
using Microsoft.EntityFrameworkCore;

namespace Demetra.Aggregates.Products
{
    [ExtendObjectType(Name = "Query")]
    public class ProductQueries
    {
        [UseApplicationDbContext]
        public Task<List<Product>> GetProductsAsync([ScopedService] ApplicationDbContext context) => context.Products.ToListAsync();
        
        public Task<Product> GetProductAsync(
            [ID(nameof(Product))]Guid id,
            ProductByIdDataLoader dataLoader,
            CancellationToken cancellationToken) =>
            dataLoader.LoadAsync(id, cancellationToken);
    }
}