using System;
using System.IO;
using System.Threading.Tasks;
using IdentityServer.Infrastructure;
using IdentityServer4.EntityFramework.DbContexts;
using Libraries.IHostExtensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

namespace IdentityServer
{
    static class Program
    {
        static readonly string Env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

        static IConfiguration Configuration { get; } = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", true, true)
            .AddEnvironmentVariables()
            .Build();

        static async Task Main(string[] args)
        {
            var minLevel = string.Equals(Env, "Debug", StringComparison.InvariantCultureIgnoreCase) || string.Equals(Env, "Development", StringComparison.InvariantCultureIgnoreCase)
                ? LogEventLevel.Information
                : LogEventLevel.Warning;

            Log.Logger = new LoggerConfiguration()
                .Enrich.WithMachineName()
                .Enrich.WithEnvironmentName()
                .Enrich.WithEnvironmentUserName()
                .Enrich.WithApplicationName()
                .WriteTo.Console()
                .WriteTo.Seq(Configuration["Seq:Host"], minLevel)
                .CreateLogger();

            try
            {
                Log.Information("IdentityServer Starting");

                var host = CreateHostBuilder(args).Build();

                await host.MigrateAsync<ApplicationDbContext>(async (context, services) => await new ApplicationDbContextSeed().SeedAsync(context));
                await host.MigrateAsync<ConfigurationDbContext>(async (context, _) => { await new ConfigurationDbContextSeed().SeedAsync(context); });
                await host.MigrateAsync<PersistedGrantDbContext>((_, __) => Task.CompletedTask);
                
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

        // https://docs.microsoft.com/en-us/aspnet/core/security/docker-compose-https?view=aspnetcore-5.0
        // https://docs.microsoft.com/en-us/aspnet/core/security/enforcing-ssl?view=aspnetcore-5.0&tabs=visual-studio
        // https://docs.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-5.0&tabs=windows
        static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((context, builder) =>
                {
                    if (context.HostingEnvironment.IsDevelopment())
                    {
                        // Add by default in Development
                        // builder.AddUserSecrets(Assembly.GetExecutingAssembly());
                    }
                })
                .ConfigureWebHostDefaults(webBuilder => webBuilder.UseStartup<Startup>())
                .ConfigureLogging(builder => builder.AddSerilog(Log.Logger, true))
                .UseSerilog();
    }
}