using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

namespace AlbyOnContainers.Pollon
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
            
            Log.Logger.Information($"CONNECTION_STRING: {connection}");

            services.AddHealthChecks()
                .AddCheck("self", () => HealthCheckResult.Healthy(), new[] {"Pollon"})
                .AddNpgSql(connection, name: "postgres", tags: new[] {"HealthDb"});

            services
                .AddHealthChecksUI(setup =>
                {
                    setup.AddHealthCheckEndpoint("IdentityServer", Configuration["HealthChecks:IdentityServerUrl"]);
                    setup.AddHealthCheckEndpoint("Hermes", Configuration["HealthChecks:HermesUrl"]);
                    setup.AddHealthCheckEndpoint("Pollon", Configuration["HealthChecks:PollonUrl"]);
                    setup.SetEvaluationTimeInSeconds(int.Parse(Configuration["HealthChecks:SetEvaluationTimeInSeconds"]));
                    setup.SetMinimumSecondsBetweenFailureNotifications(int.Parse(Configuration["HealthChecks:SetMinimumSecondsBetweenFailureNotifications"]));
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
                endpoints.MapHealthChecks("/pollon/healthz", new HealthCheckOptions
                {
                    Predicate = _ => true,
                    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                });

                endpoints.MapHealthChecksUI();
            });
        }
    }
}