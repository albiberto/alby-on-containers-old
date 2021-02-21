// ReSharper disable InconsistentNaming

using Hermes.Options;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Hermes.IoC
{
    public static class IServiceCollectionExtensions
    {
        public static void AddHermesChecks(this IServiceCollection services, HealthChecksConfiguration configuration)
        {
            services
                .AddHealthChecks()
                .AddCheck(configuration?.Self?.Name ?? "Hermes", () => HealthCheckResult.Healthy(),
                    configuration?.Self?.Tags ?? new[] {"hermes", "service", "debug"});

            services.AddMassTransitHostedService(true);
        }
    }
}