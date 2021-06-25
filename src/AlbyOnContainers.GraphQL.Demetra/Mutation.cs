using System.Threading.Tasks;
using Demetra.Infrastructure;
using Demetra.Model;
using HotChocolate;

namespace Demetra
{
    public class Mutation
    {
        public async Task<AddProductPayload> AddProductAsync(AddProductInput input, [Service] ApplicationDbContext context)
        {
            var (name, description) = input;
            var product = new Product
            {
                Name = name,
                Description = description
            };

            context.Products?.Add(product);
            await context.SaveChangesAsync();

            return new (product);
        }
    }
}