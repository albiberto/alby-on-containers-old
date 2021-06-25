using System;
using System.Collections.Generic;

namespace Demetra.Model
{
    public class Category : EntityBase
    {
        public Category(string name, string? description, Guid? parentId = default)
        {
            Name = name;
            Description = description;
            ParentId = parentId;
        }
        
        public string? Name { get; }
        public string? Description { get; }
        public Guid? ParentId { get; }
        public Category? Parent { get; set; }
        
        readonly List<Product> _products  = new();
        public IReadOnlyCollection<Product> Products => _products;
    }
}