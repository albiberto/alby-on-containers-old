// ReSharper disable UnassignedGetOnlyAutoProperty
// ReSharper disable CollectionNeverUpdated.Global

using System;
using System.Collections.Generic;

namespace Catalog.Models
{
    public class Category : EntityBase, IAggregateRoot
    {
        public Category(Guid id, string name, string description, Guid? parentCategoryId = default) : this(name, description, parentCategoryId)
        {
            Id = id;
        }

        Category(string name, string description, Guid? parentCategoryId = default)
        {
            Name = name;
            Description = description;
            ParentCategoryId = parentCategoryId;
            Products = new HashSet<ProductAggregate>();
        }

        public string Name { get; }
        public string Description { get; }
        public Guid? ParentCategoryId { get; }
        public Category? ParentCategory { get; }

        public ICollection<ProductAggregate> Products { get; }
    }
}