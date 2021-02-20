using System;
using System.Collections.Generic;
using System.Reflection;
using IdentityServer.Infrastructure;
using IdentityServer.Models;
using IdentityServer.Options;
using IdentityServer.Publishers;
using IdentityServer.Services;
using IdentityServer4.Services;
using MassTransit;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

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

            services.AddOptions<HealthChecksOptions>()
                .Bind(configuration.GetSection("HealthChecks"));

            services.AddOptions<RabbitMQOptions>()
                .Bind(configuration.GetSection("RabbitMQ"));
        }
        
        public static void AddIdentity(this IServiceCollection services, string connection)
        {
            services.AddDbContext<ApplicationDbContext>(o =>
                o.UseNpgsql(connection, sqlOptions =>
                {
                    sqlOptions.MigrationsAssembly(AssemblyName);
                    sqlOptions.EnableRetryOnFailure(15); //Configuring Connection Resiliency: https://docs.microsoft.com/en-us/ef/core/miscellaneous/connection-resiliency 
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
                .AddAspNetIdentity<ApplicationUser>()
                .AddConfigurationStore(o =>
                {
                    o.ConfigureDbContext = builder => builder.UseNpgsql(connection,
                        sqlOptions =>
                        {
                            sqlOptions.MigrationsAssembly(AssemblyName);
                            sqlOptions.EnableRetryOnFailure(15); //Configuring Connection Resiliency: https://docs.microsoft.com/en-us/ef/core/miscellaneous/connection-resiliency 
                        });
                })
                .AddOperationalStore(o =>
                {
                    o.ConfigureDbContext = builder => builder.UseNpgsql(connection,
                        sqlOptions =>
                        {
                            sqlOptions.MigrationsAssembly(AssemblyName);
                            sqlOptions.EnableRetryOnFailure(15); //Configuring Connection Resiliency: https://docs.microsoft.com/en-us/ef/core/miscellaneous/connection-resiliency 
                        });
                })
                .Services.AddTransient<IProfileService, ProfileService>();
        }

        public static void AddHealthChecks(this IServiceCollection services, string connection)
        {
            var options = (services.BuildServiceProvider().GetService<IOptions<HealthChecksOptions>>())?.Value;

            services.AddHealthChecks()
                .AddCheck(options?.Self?.Name ?? "self", () => HealthCheckResult.Healthy(), options?.Self?.Tags ?? new []{ "identity", "service", "identityserver4", "debug" } )
                .AddNpgSql(connection, name: options?.NpgSql?.Name ?? "postgres", tags: options?.NpgSql?.Tags ?? new []{ "identity", "db", "postgres", "debug" });
        }

        public static void AddMassTransit(this IServiceCollection services)
        {
            var options = (services.BuildServiceProvider().GetService<IOptions<RabbitMQOptions>>())?.Value;
            
            services.AddMassTransit(x =>
            {
                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host(options?.Host ?? "localhost", config =>
                    {
                        config.Username(options?.Username ?? "guest");
                        config.Password(options?.Password ?? "guest");
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