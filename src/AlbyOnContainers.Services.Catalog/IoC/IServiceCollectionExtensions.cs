using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Catalog.IoC
{
    // ReSharper disable once InconsistentNaming
    public static class IServiceCollectionExtensions
    {
        public static void AddHealthChecks(this IServiceCollection services, string connection, Options options)
        {
            var selfName = options?.Self?.Name ?? "self";
            var selfTags = options?.Self?.Tags ?? new[] { "catalog", "api", "graphQL" };

            var postgresName = options?.NpgSql?.Name ?? "postgres";
            var postgresTags = options?.NpgSql?.Tags ?? new[] { "catalog", "lucifer", "db", "postgres" };

            services.AddHealthChecks()
                .AddCheck(selfName, () => HealthCheckResult.Healthy(), selfTags)
                .AddNpgSql(connection, name: postgresName, tags: postgresTags);
        }
    }
}