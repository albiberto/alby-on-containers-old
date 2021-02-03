using System;
using System.IO;
using System.Threading.Tasks;
using Autofac.Extensions.DependencyInjection;
using Catalog.Infrastructure;
using Libraries.IHostExtensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

namespace Catalog
{
    static class Program
    {
        static readonly string _env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";

        static IConfiguration Configuration { get; } = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();

        static async Task Main(string[] args)
        {
            var minLevel = string.Equals(_env, "Development", StringComparison.InvariantCultureIgnoreCase) || string.Equals(_env, "Staging", StringComparison.InvariantCultureIgnoreCase)
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
                Log.Information("Catalog.Api Starting");

                var host = CreateHostBuilder(args).Build();

                await host.MigrateAsync<LuciferContext>(async (context, _) => await new LuciferDbContextSeed().SeedAsync(context));

                Log.Information("Migrations Applied");

                await host.RunAsync();

                Log.Information("Catalog.Api Started");
            }
            catch (Exception e)
            {
                Log.Fatal(e, "Catalog.Api Fatal Failed!");
            }
            finally
            {
                Log.Information("Catalog.Api Stopping!");
                Log.CloseAndFlush();
            }
        }

        static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseServiceProviderFactory(new AutofacServiceProviderFactory())
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                })
                .ConfigureLogging(builder => builder.AddSerilog(Log.Logger, dispose: true))
                .UseSerilog();
    }
}