using System.Net.Http;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;

namespace Pollon
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            var configuration = new Configuration();
            Configuration.GetSection("HealthChecks").Bind(configuration);
            
            var connection = Configuration.GetConnectionString("DefaultDatabase");

            services.AddHealthChecks()
                .AddCheck(configuration.Checks.Self.Name, () => HealthCheckResult.Healthy(), configuration.Checks.Self.Tags)
                .AddNpgSql(connection, name: configuration.Checks.NpgSql.Name, tags: configuration.Checks.NpgSql.Tags);

            services.AddHealthChecksUI(setup =>
                {
                    setup.SetEvaluationTimeInSeconds(configuration.EvaluationTimeInSeconds);
                    setup.SetMinimumSecondsBetweenFailureNotifications(configuration.MinimumSecondsBetweenFailureNotifications);

                    setup.UseApiEndpointHttpMessageHandler(_ => new HttpClientHandler
                    {
                        ServerCertificateCustomValidationCallback = (_, __, ___, ____) => true
                    });
                    
                    foreach (var endpoint in configuration.Endpoints)
                    {
                        setup.AddHealthCheckEndpoint(endpoint.Name, endpoint.Url);
                    }
                })
                .AddPostgreSqlStorage(connection);
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment()) app.UseDeveloperExceptionPage();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHealthChecks("/healthz", new HealthCheckOptions
                {
                    Predicate = _ => true,
                    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                });

                endpoints.MapHealthChecksUI();
            });
        }
    }
}