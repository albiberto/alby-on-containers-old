using System;
using Catalog.Types;
using Microsoft.Extensions.DependencyInjection;

namespace Catalog
{
    public class Schema : GraphQL.Types.Schema
    {
        public Schema(IServiceProvider services) : base(services)
        {
            Query = services.GetRequiredService<Query>();
            Mutation = services.GetRequiredService<Mutation>();
        }
    }
}