using System;
using System.Threading.Tasks;
using Demetra.Extensions;
using Demetra.Infrastructure;
using Demetra.Model;
using HotChocolate;
using HotChocolate.Types;

namespace Demetra.Aggregates.Products
{
    [ExtendObjectType(Name = "Mutation")]
    public class ProductMutations
    {
        [UseApplicationDbContext]
        public async Task<Model.AddProductPayload> AddProductAsync(AddProductInput input, [ScopedService] ApplicationDbContext context)
        {
            var (name, description, categoryId) = input;
            var product = new Product(name, description, categoryId);

            await context.Products.AddAsync(product);
            await context.SaveChangesAsync();

            return new(product);
        }
    }
}