using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

namespace AlbyOnContainers.Pollon
{
    public class Program
    {
        static readonly string Env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

        static IConfiguration Configuration { get; } = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", false, true)
            .AddEnvironmentVariables()
            .Build();

        public static async Task Main(string[] args)
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
                Log.Information("Pollon Stopping!");
                Log.CloseAndFlush();
            }
        }

        static IHost CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); })
                .ConfigureLogging(builder => { builder.AddSerilog(Log.Logger, true); })
                .UseSerilog()
                .Build();
    }
}