using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Routing;

namespace IdentityServer.Extensions
{
    // ReSharper disable once InconsistentNaming
    public static class IEndpointRouteBuilderExtensions
    {
        public static void MapHealthChecks(this IEndpointRouteBuilder builder)
        {
            builder.MapHealthChecks("/healthz", new HealthCheckOptions
            {
                Predicate = _ => true,
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
            });
        }
    }
}