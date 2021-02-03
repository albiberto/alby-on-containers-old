// ReSharper disable UnassignedGetOnlyAutoProperty
// ReSharper disable CollectionNeverUpdated.Global

using System;

namespace Catalog.Models
{
    public class AttrDesc : EntityBase, IAggregateRoot
    {
        public AttrDesc(Guid? id, string description, Guid productId, Guid attributeId) : this(description, productId, attributeId)
        {
            Id = id;
        }

        AttrDesc(string description, Guid productId, Guid attributeId)
        {
            Description = description;
            ProductId = productId;
            AttributeId = attributeId;
        }

        public string Description { get; private set; }

        public Guid ProductId { get; }
        public ProductAggregate? Product { get; }
        
        public Guid AttributeId { get; private set;}
        public AttrAggregate? Attribute { get; }
    }
}