using System.Linq;
using IdentityServer.Extensions;
using IdentityServer.Infrastructure;
using IdentityServer.Infrastructure.Seeds;
using IdentityServer.IoC;
using IdentityServer.Options;
using IdentityServer4;
using IdentityServer4.EntityFramework.DbContexts;
using Libraries.IHostExtensions;
using Microsoft.AspNetCore.Authorization.Infrastructure;
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

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var (healthChecksConfiguration, rabbitMqConfiguration, connection) = GetConfiguration();

            services.AddIdentity(connection);
            services.AddIdentityServer(connection);

            services.AddHealthChecks(connection, healthChecksConfiguration);

            services.AddMassTransit(rabbitMqConfiguration);

            services.AddAuthentication()
                .AddGoogle(options =>
                {
                    options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;

                    options.ClientId = "516459164098-dm04ij4omekj0aii8gntjm5neujul5tn.apps.googleusercontent.com";
                    options.ClientSecret = "1PNiGZSk1hjENrxnFdTUyHKY";
                })
                .AddFacebook(options =>
                {
                    options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;

                    options.AppId = "191746359619007";
                    options.AppSecret = "78a544dede886fe61f885e558980e6d4";
                    options.AccessDeniedPath = "/AccessDeniedPathInfo";
                });

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

                    services.AddAuthorization(options =>
                    {
                        options.AddPolicy("All", policy =>
                            policy.Requirements.Add(new RolesAuthorizationRequirement(new[] {"Admin", "User"})));
                    });

                    services.AddCors(options =>
                    {
                        options.AddPolicy("AlbyPolicy",
                            builder => { builder.AllowAnyOrigin().WithMethods("GET", "POST").AllowAnyHeader(); });
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
            
            app.UseIdentityServer();
            // Fix a problem with chrome. Chrome enabled a new feature "Cookies without SameSite must be secure", 
            // the coockies shold be expided from https, but in eShop, the internal comunicacion in aks and docker compose is http.
            // To avoid this problem, the policy of cookies shold be in Lax mode.
            
            app.UseCookiePolicy(new() { MinimumSameSitePolicy = SameSiteMode.Lax });
            // app.UseAuthorization();
            
            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapIdentityHealthChecks();
                endpoints.MapControllerRoute();
            });
        }
    }
}