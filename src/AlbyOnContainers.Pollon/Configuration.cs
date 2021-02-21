using System.Collections.Generic;

namespace Pollon
{
    public class Configuration
    {
        public IEnumerable<Endpoint> Endpoints { get; set; }
        public int MinimumSecondsBetweenFailureNotifications { get; set; }
        public int EvaluationTimeInSeconds { get; set; }
        public Checks Checks { get; set; }
    }

    public class Endpoint
    {
        public string Name { get; set; }
        public string Url { get; set; }
    }

    public class Checks
    {
        public Check Self { get; set; }
        public Check NpgSql { get; set; }
    }
    
    public class Check
    {
        public string Name { get; set; }
        public IReadOnlyCollection<string> Tags { get; set; }
    }
}