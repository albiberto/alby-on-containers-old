using System;
using System.Linq;
using Catalog.Infrastructure;
using Catalog.Models;
using GraphQL.DataLoader;
using GraphQL.Types;
using GraphQL.Utilities;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Types
{
    public sealed class AttributeDescriptionType : ObjectGraphType<AttrDesc>
    {
        public AttributeDescriptionType(IServiceProvider provider, IDataLoaderContextAccessor dataLoader)
        {
            Name = "AttributeDescription";
            Description = "The description of an attribute.";

            Field(d => d.Id).Description("The id of attribute description.");
            Field(d => d.Description).Description("The description of the associated attribute.");
            Field(d => d.AttributeId).Description("The id of associate attribute");
            Field(d => d.ProductId).Description("The id of associate product");

            Field<AttributeType, Attr>()
                .Name("attribute")
                .Description("The atrribute of the description.")
                .ResolveAsync(context =>
                {
                    var loader = dataLoader.Context.GetOrAddBatchLoader<Guid, Attr>("GetAttributesByIds", async ids =>
                        await provider.GetRequiredService<LuciferContext>()
                            .Attrs
                            .Where(attribute => ids.Contains(attribute.Id))
                            .ToDictionaryAsync(e => e.Id));

                    return loader.LoadAsync(context.Source.AttributeId);
                });
        }
    }
}