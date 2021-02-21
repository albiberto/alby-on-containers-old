using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Pollon
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            try
            {
                var host = CreateHostBuilder(args).Build();
                Log.Information("Pollon Starting");

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

        static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
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