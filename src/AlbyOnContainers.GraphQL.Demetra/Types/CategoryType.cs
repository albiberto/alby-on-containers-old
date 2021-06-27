using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Demetra.DataLoader;
using Demetra.Extensions;
using Demetra.Infrastructure;
using Demetra.Model;
using HotChocolate;
using HotChocolate.Resolvers;
using HotChocolate.Types;
using Microsoft.EntityFrameworkCore;

namespace Demetra.Types
{
    public class CategoryType: ObjectType<Category>
    {
        protected override void Configure(IObjectTypeDescriptor<Category> descriptor)
        {
            descriptor
                .ImplementsNode()
                .IdField(t => t.Id)
                .ResolveNode((ctx, id) => ctx.DataLoader<CategoryByIdDataLoader>().LoadAsync(id, ctx.RequestAborted));
            
            descriptor
                .Field(t => t.Products)
                .ResolveWith<ProductResolvers>(t => t.GetSessionsAsync(default!, default!, default!, default))
                .UseDbContext<ApplicationDbContext>()
                .Name("products");
            
            descriptor
                .Field(t => t.Parent)
                .ResolveWith<ProductResolvers>(t => t.GetCategoryAsync(default!, default!, default));

            descriptor
                .Field(t => t.ParentId)
                .ID(nameof(Category));
        }
        
        class ProductResolvers
        {
            public async Task<IEnumerable<AttrDescr>> GetSessionsAsync(EntityBase product, [ScopedService] ApplicationDbContext dbContext,
                AttrDescrByIdDataLoader attrDescrById, CancellationToken cancellationToken)
            {
                var attrDescrIds = await dbContext.Products
                    .Where(s => s.Id == product.Id)
                    .SelectMany(s => s.Descrs.Select(d => d.Id))
                    .ToArrayAsync(cancellationToken);

                return await attrDescrById.LoadAsync(attrDescrIds, cancellationToken);
            }
            public async Task<Category?> GetCategoryAsync(Category category, CategoryByIdDataLoader categoryById, CancellationToken cancellationToken)
            {
                return category.ParentId.HasValue 
                    ? await categoryById.LoadAsync(category.ParentId.Value, cancellationToken)
                    : default;
            }
        }
    }
}