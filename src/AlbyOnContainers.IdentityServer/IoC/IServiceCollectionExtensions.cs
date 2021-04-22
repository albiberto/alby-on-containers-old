using System;
using System.Reflection;
using IdentityServer.Infrastructure;
using IdentityServer.Models;
using IdentityServer.Options;
using IdentityServer.Publishers;
using IdentityServer.Services;
using IdentityServer4.Services;
using MassTransit;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace IdentityServer.IoC
{
    // ReSharper disable once InconsistentNaming
    public static class IServiceCollectionExtensions
    {
        static readonly string AssemblyName = Assembly.GetExecutingAssembly().GetName().Name;

        public static void AddOptions(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddOptions<TokenLifetimeOptions>()
                .Bind(configuration.GetSection("TokenLifetime"))
                .ValidateDataAnnotations();

            services.AddOptions<EmailOptions>()
                .Bind(configuration.GetSection("Email"));
        }

        public static void AddIdentity(this IServiceCollection services, string connection)
        {
            services.AddDbContext<ApplicationDbContext>(o =>
                o.UseNpgsql(connection, sqlOptions =>
                {
                    sqlOptions.MigrationsAssembly(AssemblyName);
                    sqlOptions.EnableRetryOnFailure(
                        15); //Configuring Connection Resiliency: https://docs.microsoft.com/en-us/ef/core/miscellaneous/connection-resiliency 
                }));

            services.AddIdentity<ApplicationUser, IdentityRole>(o => o.SignIn.RequireConfirmedAccount = true)
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();
        }

        public static void AddIdentityServer(this IServiceCollection services, string connection)
        {
            // Adds AlbyOnContainers.IdentityServer
            services.AddIdentityServer(x =>
                {
                    x.IssuerUri = "null";
                    x.Authentication.CookieLifetime = TimeSpan.FromHours(2);
                })
                // .AddSigningCredential(CertificateManager.Get())
                .AddDeveloperSigningCredential()
                .AddAspNetIdentity<ApplicationUser>()
                .AddConfigurationStore(o =>
                {
                    o.ConfigureDbContext = builder => builder.UseNpgsql(connection,
                        sqlOptions =>
                        {
                            sqlOptions.MigrationsAssembly(AssemblyName);
                            sqlOptions.EnableRetryOnFailure(
                                15); //Configuring Connection Resiliency: https://docs.microsoft.com/en-us/ef/core/miscellaneous/connection-resiliency 
                        });
                })
                .AddOperationalStore(o =>
                {
                    o.ConfigureDbContext = builder => builder.UseNpgsql(connection,
                        sqlOptions =>
                        {
                            sqlOptions.MigrationsAssembly(AssemblyName);
                            sqlOptions.EnableRetryOnFailure(
                                15); //Configuring Connection Resiliency: https://docs.microsoft.com/en-us/ef/core/miscellaneous/connection-resiliency 
                        });
                })
                .Services.AddTransient<IProfileService, ProfileService>();
        }

        public static void AddHealthChecks(this IServiceCollection services, string connection, HealthChecksConfiguration configuration)
        {
            services.AddHealthChecks()
                .AddCheck(configuration?.Self?.Name ?? "self", () => HealthCheckResult.Healthy(),
                    configuration?.Self?.Tags ?? new[] {"identity", "service", "identityserver4", "debug"})
                .AddNpgSql(connection, name: configuration?.NpgSql?.Name ?? "postgres",
                    tags: configuration?.NpgSql?.Tags ?? new[] {"identity", "db", "postgres", "debug"});
        }

        public static void AddMassTransit(this IServiceCollection services, RabbitMQConfiguration configuration)
        {
            services.AddMassTransit(x =>
            {
                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host(configuration?.Host ?? "localhost", c =>
                    {
                        c.Username(configuration?.Username ?? "guest");
                        c.Password(configuration?.Password ?? "guest");
                    });

                    cfg.ExchangeType = "fanout";
                });
            });

            services.AddMassTransitHostedService(true);
        }

        public static void AddCustom(this IServiceCollection services)
        {
            services.AddScoped<IEmailPublisher, EmailPublisher>();
            services.AddTransient<IRedirectService, RedirectService>();
        }
    }
}