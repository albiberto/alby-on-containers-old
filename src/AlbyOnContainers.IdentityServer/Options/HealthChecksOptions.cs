using Libraries.Options;

namespace IdentityServer.Options
{
    public class HealthChecksOptions
    {
        public Check Self { get; set; }
        public Check NpgSql { get; set; }
    }
}