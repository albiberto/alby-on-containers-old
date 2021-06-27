using System;
using System.Collections.Generic;

namespace Demetra.Model
{
    public class Product : EntityBase
    {
        public Product(string name, string? description, Guid categoryId)
        {
            Name = name;
            Description = description;
            CategoryId = categoryId;
        }

        public string? Name { get; }
        public string? Description { get; }

        readonly List<AttrDescr> _descrs  = new();
        public IReadOnlyCollection<AttrDescr> Descrs => _descrs;
        
        
        public Category? Category { get; set; }
        public Guid CategoryId { get; }
    }
}