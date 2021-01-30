using System;
using System.Collections.Generic;

namespace Catalog.Models
{
    public class Category : EntityBase
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public Guid? ParentCategoryId { get; set; }
        public Category ParentCategory { get; set; }

        public IEnumerable<Product> Products { get; set; }
    }
}