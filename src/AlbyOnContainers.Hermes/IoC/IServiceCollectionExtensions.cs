// ReSharper disable InconsistentNaming

using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Hermes.IoC
{
    public static class IServiceCollectionExtensions
    {
        public static void AddHealthChecks(this IServiceCollection services, IConfiguration configuration)
        {
            var options = new Options();
            configuration.GetSection("HealthChecks").Bind(options);

            services
                .AddHealthChecks()
                .AddCheck(options.Self.Name, () => HealthCheckResult.Healthy(), options.Self.Tags);

            services.AddMassTransitHostedService(waitUntilStarted: true);
        }
    }
}