using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Catalog.Models
{
    public class Attr: EntityBase
    {
        [Required] public string Name { get; set; }

        public IEnumerable<AttrDesc> Descriptions { get; set; }
    }
}