using System;
using System.IO;
using System.Threading.Tasks;
using AlbyOnContainers.Hermes.IoC;
using AlbyOnContainers.Hermes.Senders;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using HealthChecks.UI.Client;
using MassTransit;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using IHost = Microsoft.Extensions.Hosting.IHost;

namespace AlbyOnContainers.Hermes
{
    public class Program
    {
        static readonly string Env = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");

        static IConfiguration Configuration { get; } = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", false, true)
            .AddEnvironmentVariables()
            .Build();

        static async Task Main(string[] args)
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

        static IHostBuilder CreateHostBuilder(string[] args) => Host.CreateDefaultBuilder(args)
            .UseServiceProviderFactory(new AutofacServiceProviderFactory())
            .ConfigureServices((hostContext, services) =>
            {
                services.AddMassTransitHostedService(true);

                services.AddHealthChecks()
                    .AddCheck("self", () => HealthCheckResult.Healthy(), new[] {"Hermes"});

                services.AddOptions<EmailSender.Options>()
                    .Bind(Configuration.GetSection("EmailOptions"));
            })
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.Configure(app =>
                {
                    app.UseRouting();
                    
                    app.UseEndpoints(endpoints =>
                    {
                        endpoints.MapHealthChecks("/hermes/healthz", new HealthCheckOptions
                        {
                            Predicate = _ => true,
                            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                        });
                    });
                });
            })
            .ConfigureContainer<ContainerBuilder>(builder => { builder.RegisterModule(new HermesModule(Configuration)); })
            .ConfigureLogging(builder => { builder.AddSerilog(Log.Logger, true); })
            .UseSerilog();
    }
}