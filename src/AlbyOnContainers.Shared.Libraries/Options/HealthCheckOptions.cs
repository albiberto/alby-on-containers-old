// ReSharper disable ClassNeverInstantiated.Global

using System.Collections.Generic;

namespace Libraries.Options
{
    public class Check
    {
        public string Name { get; set; }
        public IEnumerable<string> Tags { get; set; }
    }
}