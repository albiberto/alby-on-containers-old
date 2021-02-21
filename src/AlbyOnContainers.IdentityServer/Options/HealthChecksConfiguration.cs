using System.Collections.Generic;

namespace IdentityServer.Options
{
    public class HealthChecksConfiguration
    {
        public Check Self { get; set; }
        public Check NpgSql { get; set; }

        public class Check
        {
            public string Name { get; set; }
            public IReadOnlyCollection<string> Tags { get; set; }
        }
    }
}