using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Catalog.Models
{
    public class Product : EntityBase
    {
        public string Name { get; set; }
        public Guid CategoryId { get; set; }
        public Category Category { get; set; }
        public ICollection<AttrDesc> Descriptions { get; set; } = new List<AttrDesc>();
    }
}