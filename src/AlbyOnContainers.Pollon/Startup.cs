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
            var connection = Configuration.GetConnectionString("Pollon");
            var options = new Options();
            Configuration.GetSection("HealthChecks").Bind(options);

            services.AddHealthChecks()
                .AddCheck(options.Checks.Self.Name, () => HealthCheckResult.Healthy(), options.Checks.Self.Tags)
                .AddNpgSql(connection, name: options.Checks.NpgSql.Name, tags: options.Checks.NpgSql.Tags);

            services.AddHealthChecksUI(setup =>
                {
                    setup.SetEvaluationTimeInSeconds(options.EvaluationTimeInSeconds);
                    setup.SetMinimumSecondsBetweenFailureNotifications(options.MinimumSecondsBetweenFailureNotifications);

                    foreach (var endpoint in options.Endpoints)
                    {
                        setup.AddHealthCheckEndpoint(endpoint.Name, endpoint.Url);
                    }
                })
                .AddPostgreSqlStorage(connection);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
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