using System.Linq;
using System.Threading.Tasks;
using Catalog.Infrastructure;
using Catalog.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Catalog.Repository
{
    public class ProductRepository : RepositoryBase<ProductAggregate>
    {
        public ProductRepository(LuciferContext context) : base(context)
        {
        }

        public override async Task<EntityEntry<ProductAggregate>> UpdateAsync(ProductAggregate productAggregate)
        {
            var result = _context.Products!.Update(productAggregate);

            var descriptions = await _context.AttrDescs!
                .AsNoTracking()
                .Where(desc => desc.ProductId == productAggregate.Id)
                .ToListAsync();

            var tobeDeleted = descriptions.Except(productAggregate.Descriptions);

            _context.RemoveRange(tobeDeleted);

            return result;
        }
    }
}