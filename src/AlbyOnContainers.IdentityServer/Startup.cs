using System.Linq;
using IdentityServer.Extensions;
using IdentityServer.Infrastructure;
using IdentityServer.Infrastructure.Seeds;
using IdentityServer.IoC;
using IdentityServer.Options;
using IdentityServer4.EntityFramework.DbContexts;
using Libraries.IHostExtensions;
using Microsoft.AspNetCore.Builder;
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

            if (!_env.IsProduction())
            {
                services.AddSingleton<IDbContextSeed<ApplicationDbContext>, ApplicationDbContextSeed>();
                services.AddSingleton<IDbContextSeed<ConfigurationDbContext>, ConfigurationDbContextSeed>();
            }
            
            services.AddCors(options =>
            {
                options.AddPolicy("AlbyPolicy",
                    builder =>
                    {
                        builder.AllowAnyOrigin().WithMethods("GET", "POST").AllowAnyHeader();
                    });
            });

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
            app.UseCors("AlbyPolicy");
            
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

            if (!env.IsProduction()) app.UseDeveloperExceptionPage();
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            
            app.UseRouting();
            app.UseIdentityServer();
            app.UseAuthorization();
            
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapIdentityHealthChecks();
                endpoints.MapControllerRoute();
            });
        }
    }
}