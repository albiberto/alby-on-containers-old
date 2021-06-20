using IdentityServer.Extensions;
using IdentityServer.IoC;
using IdentityServer.Options;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
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

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddOptions(Configuration);
            var connection = Configuration.GetConnectionString("DefaultDatabase");
            
            AddHttpsServices(services);
            AddSecurityServices(services, connection);
            AddCustomServices(services, connection);

            services.AddControllersWithViewsAndFilters();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app)
        {
            app.UseCors("AlbyPolicy");
            app.UseProxy();

            if (!_env.IsProduction()) app.UseDeveloperExceptionPage();
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            
            UseIdentityServer(app);

            app.UseRouting();
            app.UseAuthorization();

            app.UseSecurityHeaders();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapIdentityHealthChecks();
                endpoints.MapControllerRoute();
            });
        }
        
        void AddCustomServices(IServiceCollection services, string connection)
        {
            var healthChecksConfig = new HealthChecksConfiguration();
            Configuration.GetSection("HealthChecks").Bind(healthChecksConfig);

            var rabbitMqConfig = new RabbitMQConfiguration();
            Configuration.GetSection("RabbitMQ").Bind(rabbitMqConfig);
            
            services.AddHealthChecks(connection, healthChecksConfig);
            services.AddMassTransit(rabbitMqConfig);
            services.AddCustomServices(_env);
        }

        void AddHttpsServices(IServiceCollection services)
        {
            services.AddHttpsRedirection(_env);
            services.AddHsts();
            services.AddReverseProxy();
        }

        static void AddSecurityServices(IServiceCollection services, string connection)
        {
            services.AddIdentity(connection);
            services.AddIdentityServer(connection);
            
            services.AddAuthenticationWithExternalProviders();
            services.AddAuthorizationPolicies();
            services.AddCorsPolicies();
        }
        
        static void UseIdentityServer(IApplicationBuilder app)
        {
            app.UseIdentityServer();

            // Fix a problem with chrome. Chrome enabled a new feature "Cookies without SameSite must be secure", 
            // the coockies shold be expided from https, but in eShop, the internal comunicacion in aks and docker compose is http.
            // To avoid this problem, the policy of cookies shold be in Lax mode.
            app.UseCookiePolicy(new() {MinimumSameSitePolicy = SameSiteMode.Lax});
        }
    }
}