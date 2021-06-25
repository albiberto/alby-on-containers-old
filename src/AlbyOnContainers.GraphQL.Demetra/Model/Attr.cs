using System.Collections.Generic;

namespace Demetra.Model
{
    public class Attr : EntityBase
    {
        public Attr(string name, string? description)
        {
            Name = name;
            Description = description;
        }
        
        public string Name { get; }
        public string? Description { get; }
        
        readonly List<AttrDescr> _descrs  = new();
        public IReadOnlyCollection<AttrDescr> Descrs => _descrs;
    }
}