using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Catalog.IoC
{
    // ReSharper disable once InconsistentNaming
    public static class IServiceCollectionExtensions
    {
        public static void AddHealthChecks(this IServiceCollection services, string connection, Configuration configuration)
        {
            var selfName = configuration?.Self?.Name ?? "self";
            var selfTags = configuration?.Self?.Tags ?? new[] { "catalog", "api", "graphQL" };

            var postgresName = configuration?.NpgSql?.Name ?? "postgres";
            var postgresTags = configuration?.NpgSql?.Tags ?? new[] { "catalog", "sherlock", "db", "postgres" };

            services.AddHealthChecks()
                .AddCheck(selfName, () => HealthCheckResult.Healthy(), selfTags)
                .AddNpgSql(connection, name: postgresName, tags: postgresTags);
        }
    }
}