using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Routing;

namespace IdentityServer.Extensions
{
    // ReSharper disable once InconsistentNaming
    public static class IEndpointRouteBuilderExtensions
    {
        public static void MapIdentityHealthChecks(this IEndpointRouteBuilder builder)
        {
            builder.MapHealthChecks("/healthz", new()
            {
                Predicate = _ => true,
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
            });
        }
        public static void MapControllerRoute(this IEndpointRouteBuilder builder)
        {
            builder.MapControllerRoute(
                "area",
                "{area:exists}/{controller=Home}/{action=Index}/{id?}");
            
            builder.MapControllerRoute(
                "default",
                "{controller=Home}/{action=Index}/{id?}",
                new[] { "IdentityServer.Controllers" }
            );
        }
    }
}