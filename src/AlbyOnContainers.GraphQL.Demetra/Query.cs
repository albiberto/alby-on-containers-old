using System.Collections.Generic;
using System.Threading.Tasks;
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
    }
}