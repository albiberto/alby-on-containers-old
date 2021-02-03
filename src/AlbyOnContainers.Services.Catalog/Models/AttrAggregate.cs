// ReSharper disable UnassignedGetOnlyAutoProperty
// ReSharper disable CollectionNeverUpdated.Global

using System;
using System.Collections.Generic;

namespace Catalog.Models
{
    public class AttrAggregate : EntityBase, IAggregateRoot
    {
        public AttrAggregate(Guid id, string name) : this(name)
        {
            Id = id;
        }

        AttrAggregate(string name)
        {
            Name = name;
            Descriptions = new HashSet<AttrDesc>();
        }

        public string Name { get; }

        public ICollection<AttrDesc> Descriptions { get; }
    }
}