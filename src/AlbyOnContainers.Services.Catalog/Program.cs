using System;
using System.Threading.Tasks;
using Autofac.Extensions.DependencyInjection;
using Catalog.Infrastructure;
using Libraries.IHostExtensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Sinks.Loki;

namespace Catalog
{
    static class Program
    {
        static async Task Main(string[] args)
        {
            try
            {
                var host = CreateHostBuilder(args).Build();
                Log.Information("Sherlock Starting");

                await host.MigrateAsync<ApplicationContext>(async (context, _) => await new ApplicationDbContextSeed().SeedAsync(context));
                Log.Information("Migrations Applied");

                await host.RunAsync();
                Log.Information("Sherlock Started");
            }
            catch (Exception e)
            {
                Log.Fatal(e, "Sherlock Fatal Failed!");
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
                .UseSerilog()
                .ConfigureWebHostDefaults(webBuilder => webBuilder.UseStartup<Startup>())
                .ConfigureLogging((host, builder) =>
                {
                    Log.Logger = new LoggerConfiguration()
                        .ReadFrom.Configuration(host.Configuration)
                        .CreateLogger();

                    builder.AddSerilog(Log.Logger, true);
                });
    }
}