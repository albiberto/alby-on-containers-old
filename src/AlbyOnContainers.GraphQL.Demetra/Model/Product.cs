using System;
using System.ComponentModel.DataAnnotations;

namespace Demetra.Model
{
    public class Product
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [StringLength(200)]
        public string? Name { get; set; }

        [StringLength(4000)]
        public string? Description { get; set; }
    }
}