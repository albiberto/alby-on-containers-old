using System;
using System.ComponentModel.DataAnnotations;

namespace Catalog.Models
{
    public class AttrDesc : EntityBase
    {
        public string Description { get; set; }

        public Guid ProductId { get; set; }
        public Product? Product { get; set; }

        public Guid AttributeId { get; set; }
        public Attr? Attribute { get; set; }
    }
}