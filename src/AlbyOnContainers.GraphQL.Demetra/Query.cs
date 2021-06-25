using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Demetra.DataLoader;
using Demetra.Extensions;
using Demetra.Infrastructure;
using Demetra.Model;
using HotChocolate;
using Microsoft.EntityFrameworkCore;

namespace Demetra
{
    public class Query
    {
        [UseApplicationDbContext]
        public Task<List<Product>> GetProductsAsync([ScopedService] ApplicationDbContext context) => context.Products.ToListAsync();
        
        public Task<Product> GetProductAsync(
            Guid id,
            ProductByIdDataLoader dataLoader,
            CancellationToken cancellationToken) =>
            dataLoader.LoadAsync(id, cancellationToken);
    }
}