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

namespace IdentityServer
{
    internal static class Program
    {
        static readonly string Env = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");

        static IConfiguration Configuration { get; } = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", false, true)
            .AddJsonFile($"appsettings.{Env}.json", true, true)
            .Build();
        
        static async Task Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(Configuration)
                .CreateLogger();
            
            try
            {
                Log.Information("Giano Starting");

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
                
                Log.Information("Giano Started");
            }
            catch (Exception e)
            {
                Log.Fatal(e, "Giano Fatal Failed!");
            }
            finally
            {
                Log.Information("Giano Stopping!");
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