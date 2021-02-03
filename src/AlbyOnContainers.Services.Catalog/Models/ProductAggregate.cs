// ReSharper disable UnassignedGetOnlyAutoProperty
// ReSharper disable CollectionNeverUpdated.Global

using System;
using System.Collections.Generic;

namespace Catalog.Models
{
    public class ProductAggregate : EntityBase
    {
        public ProductAggregate(Guid id, string name, Guid categoryId) : this(name, categoryId)
        {
            Id = id;
        }

        ProductAggregate(string name, Guid categoryId)
        {
            Name = name;
            CategoryId = categoryId;
            Descriptions = new HashSet<AttrDesc>();
        }

        public string Name { get; }
        public Guid CategoryId { get; }
        public Category? Category { get; }
        public ICollection<AttrDesc> Descriptions { get; }

        public void AddDesc(AttrDesc product) => Descriptions.Add(product);
    }
}