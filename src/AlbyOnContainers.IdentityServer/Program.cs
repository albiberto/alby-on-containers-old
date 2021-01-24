using System;
using System.IO;
using System.Threading.Tasks;
using IdentityServer.Extensions;
using IdentityServer.Infrastructure;
using IdentityServer4.EntityFramework.DbContexts;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;

namespace IdentityServer
{
    internal static class Program
    {
        static readonly string Env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

        static IConfiguration Configuration { get; } = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", false, true)
            .AddEnvironmentVariables()
            .Build();

        static async Task Main(string[] args)
        {
            var minLevel = string.Equals(Env, "Development", StringComparison.InvariantCultureIgnoreCase) || string.Equals(Env, "Stagging", StringComparison.InvariantCultureIgnoreCase)
                ? LogEventLevel.Information
                : LogEventLevel.Warning;

            Log.Logger = new LoggerConfiguration()
                .Enrich.WithMachineName()
                .Enrich.WithEnvironmentName()
                .Enrich.WithEnvironmentUserName()
                .Enrich.WithApplicationName()
                .WriteTo.Console()
                .WriteTo.Seq(Configuration.GetConnectionString("Seq"), minLevel)
                .CreateLogger();

            try
            {
                Log.Information("IdentityServer Starting");

                var host = CreateHostBuilder(args);

                Log.Information("Starting Migrations");

                await host.MigrateAsync<ApplicationDbContext>(async (context, services) =>
                {
                    var logger = services.GetService<ILogger<ApplicationDbContextSeed>>();

                    await new ApplicationDbContextSeed().SeedAsync(context, logger);
                });

                await host.MigrateAsync<ConfigurationDbContext>(async (context, _) => { await new ConfigurationDbContextSeed().SeedAsync(context); });

                await host.MigrateAsync<PersistedGrantDbContext>((_, __) => Task.CompletedTask);

                Log.Information("Migrations Applied");

                await host.RunAsync();

                Log.Information("IdentityServer Started");
            }
            catch (Exception e)
            {
                Log.Fatal(e, "IdentityServer Fatal Failed!");
            }
            finally
            {
                Log.Information("IdentityServer Stopping!");
                Log.CloseAndFlush();
            }
        }

        static IHost CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); })
                .ConfigureLogging(builder => builder.AddSerilog(Log.Logger, true))
                .UseSerilog()
                .Build();
    }
}