using System;
using System.IO;
using System.Threading.Tasks;
using Autofac.Extensions.DependencyInjection;
using Catalog.Extensions;
using Catalog.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;

namespace Catalog
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
            var minLevel = string.Equals(Env, "Development", StringComparison.InvariantCultureIgnoreCase) || string.Equals(Env, "Staging", StringComparison.InvariantCultureIgnoreCase)
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

                var host = CreateHostBuilder(args).Build();

                Log.Information("Starting Migrations");

                await host.MigrateAsync<LuciferContext>((_, __) => Task.CompletedTask);
                await host.MigrateAsync<LuciferContext>(async (context, services) =>
                {
                    var logger = services.GetService<ILogger<LuciferDbContextSeed>>();

                    await new LuciferDbContextSeed().SeedAsync(context, logger);
                });

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

        static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseServiceProviderFactory(new AutofacServiceProviderFactory())
                .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); })
                .ConfigureLogging(builder => builder.AddSerilog(Log.Logger, true))
                .UseSerilog();
    }
}