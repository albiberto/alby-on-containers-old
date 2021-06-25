using System.Threading.Tasks;
using Demetra.Extensions;
using Demetra.Infrastructure;
using Demetra.Model;
using HotChocolate;

namespace Demetra
{
    public class Mutation
    {
        [UseApplicationDbContext]
        public async Task<AddProductPayload> AddProductAsync(AddProductInput input, [ScopedService] ApplicationDbContext context)
        {
            var (name, description) = input;
            var product = new Product
            {
                Name = name,
                Description = description
            };

            await context.Products.AddAsync(product);
            await context.SaveChangesAsync();

            return new(product);
        }
    }
}