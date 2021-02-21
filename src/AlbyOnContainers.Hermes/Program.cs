using System;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using HealthChecks.UI.Client;
using Hermes.IoC;
using Hermes.Options;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Hermes
{
    public static class Program
    {
        static async Task Main(string[] args)
        {
            try
            {
                var host = CreateHostBuilder(args).Build();
                Log.Information("Hermes Starting");

                await host.RunAsync();
                Log.Information("Hermes Started");
            }
            catch (Exception e)
            {
                Log.Fatal(e, "Hermes Fatal Failed!");
            }
            finally
            {
                Log.Information("Hermes Stopping!");
                Log.CloseAndFlush();
            }
        }

        static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseServiceProviderFactory(new AutofacServiceProviderFactory())
                .UseSerilog()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.Configure(app =>
                    {
                        app.UseRouting();
                        app.UseEndpoints(endpoints =>
                        {
                            endpoints.MapHealthChecks("/healthz", new HealthCheckOptions
                            {
                                Predicate = _ => true,
                                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                            });
                        });
                    });
                })
                .ConfigureLogging((host, builder) =>
                {
                    Log.Logger = new LoggerConfiguration()
                        .ReadFrom.Configuration(host.Configuration)
                        .CreateLogger();

                    builder.AddSerilog(Log.Logger, true);
                })
                .ConfigureServices((host, services) =>
                {
                    var configuration = new HealthChecksConfiguration();
                    host.Configuration.GetSection("HealthChecks").Bind(configuration);

                    services.AddHermesChecks(configuration);
                    services.AddOptions<EmailOptions>().Bind(host.Configuration?.GetSection("Email"));
                })
                .ConfigureContainer<ContainerBuilder>((host, builder) =>
                {
                    var configuration = new RabbitMQConfiguration();
                    host.Configuration.GetSection("RabbitMQ").Bind(configuration);

                    builder.RegisterModule(new HermesModule(configuration));
                });
    }
}