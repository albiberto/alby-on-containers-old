using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

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
            services
                .AddHealthChecksUI(setup =>
                {
                    setup.AddHealthCheckEndpoint("IdentityServer", Configuration["HealthChecks:IdentityServerUrl"]);
                    setup.AddHealthCheckEndpoint("Hermes", Configuration["HealthChecks:HermesUrl"]);
                    setup.SetEvaluationTimeInSeconds(int.Parse(Configuration["HealthChecks:SetEvaluationTimeInSeconds"]));
                    setup.SetMinimumSecondsBetweenFailureNotifications(int.Parse(Configuration["HealthChecks:SetMinimumSecondsBetweenFailureNotifications"]));
                })
                .AddPostgreSqlStorage(Configuration.GetConnectionString("Pollon"));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment()) app.UseDeveloperExceptionPage();

            app.UseRouting();

            app.UseEndpoints(endpoints => { endpoints.MapHealthChecksUI(); });
        }
    }
}