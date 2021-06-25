using System;
using System.Threading.Tasks;
using Demetra.Infrastructure;
using Libraries.IHostExtensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Demetra
{
    static class Program
    {
        static async Task Main(string[] args)
        {
            try
            {
                var host = CreateHostBuilder(args).Build();
                Log.Information("IdentityServer Starting");

                await host.MigrateAsync<ApplicationDbContext>();

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