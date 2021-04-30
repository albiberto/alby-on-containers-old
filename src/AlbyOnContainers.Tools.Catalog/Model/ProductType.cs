using System;
using System.Collections.Generic;

namespace AlbyOnContainers.Tools.Catalog.Model
{
    public class ProductType
    {
        public Guid Id { get; init; }
        public string Name { get; init; }
    }

    public class ProductsType
    {
        public IEnumerable<ProductType> Products { get; init; }
    }
}