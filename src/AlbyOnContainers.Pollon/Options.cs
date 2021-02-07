using System.Collections.Generic;
using Libraries.Options;

namespace Pollon
{
    public class Options
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
}