using System.Threading;
using System.Threading.Tasks;
using Demetra.DataLoader;
using Demetra.Model;
using HotChocolate.Resolvers;
using HotChocolate.Types;

namespace Demetra.Types
{
    public class AttrDescrType: ObjectType<AttrDescr>
    {
        protected override void Configure(IObjectTypeDescriptor<AttrDescr> descriptor)
        {
            descriptor
                .ImplementsNode()
                .IdField(t => t.Id)
                .ResolveNode((ctx, id) => ctx.DataLoader<AttrDescrByIdDataLoader>().LoadAsync(id, ctx.RequestAborted));
            
            descriptor
                .Field(t => t.Attr)
                .ResolveWith<SessionResolvers>(t => t.GetAttrAsync(default!, default!, default));

            descriptor
                .Field(t => t.AttrId)
                .ID(nameof(Attr));
            
            descriptor
                .Field(t => t.Product)
                .ResolveWith<SessionResolvers>(t => t.GetProductAsync(default!, default!, default));

            descriptor
                .Field(t => t.ProductId)
                .ID(nameof(Attr));
        }
        
        class SessionResolvers
        {
            public async Task<Product?> GetProductAsync(AttrDescr attrDescr, ProductByIdDataLoader productById, CancellationToken cancellationToken) => await productById.LoadAsync(attrDescr.ProductId, cancellationToken);
            public async Task<Attr?> GetAttrAsync(AttrDescr attrDescr, AttrByIdDataLoader attrById, CancellationToken cancellationToken) => await attrById.LoadAsync(attrDescr.AttrId, cancellationToken);
        }
    }
}