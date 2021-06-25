using System.Threading.Tasks;
using Demetra.Extensions;
using Demetra.Infrastructure;
using Demetra.Model;
using HotChocolate;
using HotChocolate.Types;

namespace Demetra.Products
{
    [ExtendObjectType(Name = "Mutation")]
    public class ProductMutation
    {
        [UseApplicationDbContext]
        public async Task<Model.AddProductPayload> AddProductAsync(AddProductInput input, [ScopedService] ApplicationDbContext context)
        {
            var (name, description) = input;
            var product = new Product(name, description);

            await context.Products.AddAsync(product);
            await context.SaveChangesAsync();

            return new(product);
        }
    }
}