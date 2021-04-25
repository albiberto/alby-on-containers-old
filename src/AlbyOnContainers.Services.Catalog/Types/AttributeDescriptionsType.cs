using System;
using Catalog.Models;
using Catalog.Repository;
using GraphQL.DataLoader;
using GraphQL.Types;
using Microsoft.Extensions.DependencyInjection;

namespace Catalog.Types
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public sealed class AttributeDescriptionType : ObjectGraphType<AttrDesc>
    {
        public AttributeDescriptionType()
        {
        }

        public AttributeDescriptionType(IServiceProvider provider, IDataLoaderContextAccessor dataLoader)
        {
            Name = "AttributeDescription";
            Description = "The description of an attribute.";

            Field(d => d.Id, nullable: true).Description("The id of attribute description.");
            Field(d => d.Description).Description("The description of the associated attribute.");
            Field(d => d.AttributeId).Description("The id of associate attribute");
            Field(d => d.ProductId).Description("The id of associate product");

            Field<AttributeType, AttrAggregate>()
                .Name("attribute")
                .Description("The atrribute of the description.")
                .ResolveAsync(context =>
                {
                    var loader = dataLoader.Context.GetOrAddBatchLoader<Guid, AttrAggregate>("GetAttributesByIds", async ids =>
                        await provider.GetRequiredService<AttrRepository>().GetAttributesAsync(ids));

                    return loader.LoadAsync(context.Source.AttributeId);
                });
        }
    }
}