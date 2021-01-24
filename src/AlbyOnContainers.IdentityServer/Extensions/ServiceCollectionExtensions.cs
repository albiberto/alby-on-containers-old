using System;
using IdentityServer.Certificate;
using IdentityServer.Devspaces;
using IdentityServer.Handlers;
using IdentityServer.Infrastructure;
using IdentityServer.Models;
using IdentityServer.Options;
using IdentityServer.Senders;
using IdentityServer.Services;
using IdentityServer4.Services;
using MassTransit;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace IdentityServer.Extensions
{
    public static class ServiceCollectionExtensions
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
                .AddDevspacesIfNeeded(enableDevspaces)
                .AddSigningCredential(CertificateManager.Get())
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

        public static void AddHealthCheck(this IServiceCollection services, string connection)
        {
            services.AddHealthChecks()
                .AddCheck("self", () => HealthCheckResult.Healthy())
                .AddNpgSql(connection, name: "IdentityDB-check", tags: new[] {"IdentityDB"});

            services
                .AddHealthChecksUI()
                .AddPostgreSqlStorage(connection);
        }

        public static void AddOptions(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddOptions<TokenLifetimeOptions>()
                .Bind(configuration.GetSection("TokenLifetime"))
                .ValidateDataAnnotations();

            services.AddOptions<EmailSender.Options>()
                .Bind(configuration.GetSection("SmtpClient"));

            services.AddOptions<RegisterHandler.Options>()
                .Bind(configuration.GetSection("Email"));
        }
        
        public static void AddMassTransit(this IServiceCollection services)
        {
            services.AddMassTransit(x =>
            {
                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host("localhost", config =>
                    {
                        config.Username("guest");
                        config.Password("guest");
                    });

                    cfg.ExchangeType = "fanout";
                });
            });

            services.AddMassTransitHostedService();
        }
    }
}