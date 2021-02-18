using IdentityServer.IoC;
using System.Reflection;
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
            var connection = Configuration.GetConnectionString("DefaultDatabase");
            var migrationsAssembly = Assembly.GetExecutingAssembly().GetName().Name;

            services.AddIdentity(connection, migrationsAssembly);
            services.AddIdentityServer(connection, migrationsAssembly, Configuration.GetValue("EnableDevspaces", false));
            services.AddHealthChecks(Configuration);

            services.AddOptions(Configuration);
            services.AddMassTransit(Configuration);

            services.AddCustom();

            services.AddHttpsRedirection(Configuration, _env);
            services.AddHsts(Configuration);
            services.AddReverseProxy();

            services.AddControllersWithViews();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
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
                endpoints.MapControllerRoute(
                    "default",
                    "{controller=Home}/{action=Index}/{id?}");

                endpoints.MapHealthChecks();
            });
        }
    }
}