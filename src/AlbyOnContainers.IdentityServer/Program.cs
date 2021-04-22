using System;
using System.Threading.Tasks;
using IdentityServer.Infrastructure;
using IdentityServer4.EntityFramework.DbContexts;
using Libraries.IHostExtensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace IdentityServer
{
    static class Program
    {
        static async Task Main(string[] args)
        {
            try
            {
                var host = CreateHostBuilder(args).Build();
                Log.Information("IdentityServer Starting");

                await host.MigrateAsync<ApplicationDbContext>(async (_, services) => await new ApplicationDbContextSeed().SeedAsync(services));
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
        static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .UseSerilog()
                .ConfigureWebHostDefaults(webBuilder => webBuilder.UseStartup<Startup>())
                .ConfigureAppConfiguration((host, builder) =>
                {
                    if (host.HostingEnvironment.IsDevelopment())
                    {
                        // Add by default in Development
                        // builder.AddUserSecrets(Assembly.GetExecutingAssembly());
                    }
                })
                .ConfigureLogging((host, builder) =>
                {
                    Log.Logger = new LoggerConfiguration()
                        .ReadFrom.Configuration(host.Configuration)
                        .CreateLogger();

                    builder.AddSerilog(Log.Logger, true);
                });
        }
    }
}