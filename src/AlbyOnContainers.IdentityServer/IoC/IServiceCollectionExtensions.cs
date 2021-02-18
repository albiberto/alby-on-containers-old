using System;
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
        public static void AddIdentity(this IServiceCollection services, string connection, string assembly)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(connection, sqlOptions =>
                {
                    sqlOptions.MigrationsAssembly(assembly);
                    sqlOptions.EnableRetryOnFailure(15); //Configuring Connection Resiliency: https://docs.microsoft.com/en-us/ef/core/miscellaneous/connection-resiliency 
                }));

            services.AddIdentity<ApplicationUser, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = true)
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();
        }

        public static void AddIdentityServer(this IServiceCollection services, string connection, string assembly, bool enableDevspaces)
        {
            // Adds AlbyOnContainers.IdentityServer
            services.AddIdentityServer(x =>
                {
                    x.IssuerUri = "null";
                    x.Authentication.CookieLifetime = TimeSpan.FromHours(2);
                })
                // .AddSigningCredential(CertificateManager.Get())
                .AddAspNetIdentity<ApplicationUser>()
                .AddConfigurationStore(options =>
                {
                    options.ConfigureDbContext = builder => builder.UseNpgsql(connection,
                        sqlOptions =>
                        {
                            sqlOptions.MigrationsAssembly(assembly);
                            sqlOptions.EnableRetryOnFailure(15); //Configuring Connection Resiliency: https://docs.microsoft.com/en-us/ef/core/miscellaneous/connection-resiliency 
                        });
                })
                .AddOperationalStore(options =>
                {
                    options.ConfigureDbContext = builder => builder.UseNpgsql(connection,
                        sqlOptions =>
                        {
                            sqlOptions.MigrationsAssembly(assembly);
                            sqlOptions.EnableRetryOnFailure(15); //Configuring Connection Resiliency: https://docs.microsoft.com/en-us/ef/core/miscellaneous/connection-resiliency 
                        });
                })
                .Services.AddTransient<IProfileService, ProfileService>();
        }

        public static void AddHealthChecks(this IServiceCollection services, IConfiguration configuration)
        {
            var connection = configuration.GetConnectionString("DefaultDatabase");

            var options = new HealthChecksOptions();
            configuration.GetSection("HealthChecks").Bind(options);

            services.AddHealthChecks()
                .AddCheck(options.Self.Name, () => HealthCheckResult.Healthy(), options.Self.Tags)
                .AddNpgSql(connection, name: options.NpgSql.Name, tags: options.NpgSql.Tags);
        }

        public static void AddOptions(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddOptions<TokenLifetimeOptions>()
                .Bind(configuration.GetSection("TokenLifetimeOptions"))
                .ValidateDataAnnotations();

            services.AddOptions<EmailOptions>()
                .Bind(configuration.GetSection("EmailOptions"));
        }

        public static void AddMassTransit(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddMassTransit(x =>
            {
                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host(configuration["RabbitMQ:Host"], config =>
                    {
                        config.Username(configuration["RabbitMQ:Username"]);
                        config.Password(configuration["RabbitMQ:Password"]);
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