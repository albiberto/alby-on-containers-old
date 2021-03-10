using System.Linq;
using HealthChecks.UI.Client;
using IdentityServer.IoC;
using IdentityServer.Options;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace IdentityServer
{
    public class Startup
    {
        readonly IWebHostEnvironment _env;

        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            _env = env;
            Configuration = configuration;
        }

        IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var (healthChecksConfiguration, rabbitMqConfiguration, connection) = GetConfiguration();
            
            services.AddIdentity(connection);
            services.AddIdentityServer(connection);
            
            services.AddHealthChecks(connection, healthChecksConfiguration);
            
            services.AddMassTransit(rabbitMqConfiguration);

            services.AddOptions(Configuration);
            services.AddCustom();

            services.AddHttpsRedirection(_env);
            services.AddHsts();
            services.AddReverseProxy();

            services.AddControllersWithViews();
        }

        (HealthChecksConfiguration, RabbitMQConfiguration rabbitMqConfig, string connection) GetConfiguration()
        {
            var healthChecksConfig = new HealthChecksConfiguration();
            Configuration.GetSection("HealthChecks").Bind(healthChecksConfig);

            var rabbitMqConfig = new RabbitMQConfiguration();
            Configuration.GetSection("RabbitMQ").Bind(rabbitMqConfig);

            var connection = Configuration.GetConnectionString("DefaultDatabase");
            return (healthChecksConfig, rabbitMqConfig, connection);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.Use(async (context, next) =>
            {
                var forwardedPath = context.Request.Headers["X-Forwarded-Path"].FirstOrDefault();
                if (!string.IsNullOrEmpty(forwardedPath))
                {
                    context.Request.PathBase = forwardedPath;
                }

                await next();
            });
            
            app.UseForwardedHeaders();

            if (env.IsDevelopment()) app.UseDeveloperExceptionPage();
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseIdentityServer();

            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHealthChecks("/healthz", new HealthCheckOptions
                {
                    Predicate = _ => true,
                    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                });

                endpoints.MapControllerRoute(
                    "default",
                    "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}