// ReSharper disable ClassNeverInstantiated.Global

using System.Collections.Generic;

namespace Catalog
{
    public class Options
    {
        public Check? Self { get; set; }
        public Check? NpgSql { get; set; }
        public class Check
        {
            public string Name { get; set; }
            public IReadOnlyCollection<string> Tags { get; set; }
        }
    }
}