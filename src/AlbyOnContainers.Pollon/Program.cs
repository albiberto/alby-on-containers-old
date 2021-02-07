using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

namespace Pollon
{
    public static class Program
    {
        static readonly string _env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

        static IConfiguration Configuration { get; } = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();

        public static async Task Main(string[] args)
        {
            var minLevel = string.Equals(_env, "Development", StringComparison.InvariantCultureIgnoreCase) || string.Equals(_env, "Stagging", StringComparison.InvariantCultureIgnoreCase)
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
                Log.Information("Pollon Starting");

                var host = CreateHostBuilder(args);
                await host.RunAsync();

                Log.Information("Pollon Started");
            }
            catch (Exception e)
            {
                Log.Fatal(e, "Pollon Fatal Failed!");
            }
            finally
            {
                Log.CloseAndFlush();
                Log.Information("Pollon Stopped!");
            }
        }

        static IHost CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                })
                .ConfigureLogging(builder =>
                {
                    builder.AddSerilog(Log.Logger, dispose: true);
                })
                .UseSerilog()
                .Build();
    }
}