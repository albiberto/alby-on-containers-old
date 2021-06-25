using System.Linq;
using Demetra.Infrastructure;
using Demetra.Model;
using HotChocolate;

namespace Demetra
{
    public class Query
    {
        public IQueryable<Product>? GetProducts([Service] ApplicationDbContext context) => context.Products;
    }
}