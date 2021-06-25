using System;

namespace Demetra.Model
{
    public class AttrDescr : EntityBase
    {
        public AttrDescr(string name, string? description, Guid productId, Guid attrId)
        {
            Name = name;
            Description = description;
            ProductId = productId;
            AttrId = attrId;
        }

        public string Name { get; }
        public string? Description { get; }
        
        public Guid ProductId { get; }
        public Product? Product { get; init; }
        
        public Guid AttrId { get; }
        public Attr? Attr { get; init; }
    }
}