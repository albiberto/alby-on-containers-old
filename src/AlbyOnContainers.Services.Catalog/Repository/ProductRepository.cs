using System.Linq;
using System.Threading.Tasks;
using Catalog.Infrastructure;
using Catalog.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Catalog.Repository
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class ProductRepository : RepositoryBase<ProductAggregate>
    {
        public ProductRepository(LuciferContext context) : base(context: context)
        {
        }

        public override async Task<EntityEntry<ProductAggregate>> UpdateAsync(ProductAggregate productAggregate)
        {
            var result = _context.Products!.Update(entity: productAggregate);

            var descriptions = await _context.AttrDescs!
                .AsNoTracking()
                .Where(predicate: desc => desc.ProductId == productAggregate.Id)
                .ToListAsync();

            var tobeDeleted = descriptions.Except(second: productAggregate.Descriptions);

            _context.RemoveRange(entities: tobeDeleted);

            return result;
        }
    }
}