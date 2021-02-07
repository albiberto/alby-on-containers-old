using System;
using Catalog.Types;
using GraphQL.Utilities;

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