using Autofac;
using Autofac.Extensions.DependencyInjection;
using Catalog.Infrastructure;
using Catalog.IoC;
using GraphQL.Server;
using GraphQL.Server.Ui.Altair;
using HealthChecks.UI.Client;
using IdentityServer4.AccessTokenValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Catalog
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment environment)
        {
            Configuration = configuration;
            Environment = environment;
        }

        IConfiguration Configuration { get; }
        IWebHostEnvironment Environment { get; }
        ILifetimeScope? AutofacContainer { get; set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var connection = Configuration.GetConnectionString("DefaultDatabase");

            var options = new Configuration();
            Configuration.GetSection("HealthChecks").Bind(options);

            services.AddAuthentication(o =>
                {
                    o.DefaultScheme = IdentityServerAuthenticationDefaults.AuthenticationScheme;
                    o.DefaultAuthenticateScheme = IdentityServerAuthenticationDefaults.AuthenticationScheme;
                })
                .AddIdentityServerAuthentication(o =>
                {
                    o.Authority = "https://localhost:5001";
                    o.SupportedTokens = SupportedTokens.Jwt;
                    o.RequireHttpsMetadata = false; // Note: Set to true in production
                });

            services.AddDbContext<ApplicationContext>(optionsBuilder => optionsBuilder.UseNpgsql(connection), ServiceLifetime.Transient);

            services.AddHealthChecks(connection, options);

            services.AddCors(corsOptions => corsOptions.AddPolicy("AlbyPolicy", builder => builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

            services.AddGraphQL(o => o.EnableMetrics = !Environment.IsProduction())
                .AddGraphQLAuthorization(o =>
                {
                    o.AddPolicy("AlbyTestPolicy", policy => policy.RequireAuthenticatedUser());
                    o.AddPolicy("YanierTestPolicy", policy =>
                    {
                        policy.RequireAuthenticatedUser();
                        policy.RequireRole("Admin");
                    });
                })
                .AddSystemTextJson()
                .AddWebSockets()
                .AddDataLoader();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app)
        {
            app.UseCors("AlbyPolicy");

            app.UseRouting();
            app.UseAuthentication();
            // app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHealthChecks("/healthz", new HealthCheckOptions
                {
                    Predicate = _ => true,
                    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                });
            });

            app.UseWebSockets();
            app.UseGraphQLWebSockets<Schema>();
            app.UseGraphQL<Schema>();

            if (Environment.IsEnvironment("Debug") || Environment.IsDevelopment() || Environment.IsStaging())
            {
                app.UseDeveloperExceptionPage();
                app.UseGraphQLAltair(new GraphQLAltairOptions());
            }

            AutofacContainer = app.ApplicationServices.GetAutofacRoot();
        }

        // ConfigureContainer is where you can register things directly with Autofac.
        // This runs after ConfigureServices so the things here will override registrations made in ConfigureServices.
        // Don't build the container; that gets done for you by the factory.
        public void ConfigureContainer(ContainerBuilder builder) => builder.RegisterModule(new CatalogModule());
    }
}