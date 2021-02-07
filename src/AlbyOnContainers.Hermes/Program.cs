using System;
using System.IO;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using HealthChecks.UI.Client;
using Hermes.IoC;
using Hermes.Senders;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

namespace Hermes
{
    public static class Program
    {
        static readonly string _env = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");

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
                Log.Information("Hermes Starting");

                var host = CreateHostBuilder(args).Build();
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
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddOptions<EmailSender.Options>().Bind(Configuration.GetSection("EmailOptions"));
                    services.AddHealthChecks(Configuration);
                })
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
                .ConfigureContainer<ContainerBuilder>(builder => builder.RegisterModule(new HermesModule(Configuration)))
                .ConfigureLogging(builder => builder.AddSerilog(Log.Logger, dispose: true))
                .UseSerilog();
    }
}