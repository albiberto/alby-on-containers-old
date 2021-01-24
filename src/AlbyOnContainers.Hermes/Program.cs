using System;
using System.IO;
using System.Threading.Tasks;
using AlbyOnContainers.Hermes.IoC;
using AlbyOnContainers.Hermes.Senders;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using MassTransit;
using IHost = Microsoft.Extensions.Hosting.IHost;

namespace AlbyOnContainers.Hermes
{
    public class Program
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
                Log.Information("Hermes Starting");

                var host = CreateHostBuilder(args);
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

        static IHost CreateHostBuilder(string[] args) => Host.CreateDefaultBuilder(args)
            .UseServiceProviderFactory(new AutofacServiceProviderFactory())
            .ConfigureServices((hostContext, services) =>
            {
                services.AddMassTransitHostedService();
                
                services.AddOptions<EmailSender.Options>()
                    .Bind(Configuration.GetSection("EmailConfiguration"));
            })
            .ConfigureContainer<ContainerBuilder>(builder =>
            {
                builder.RegisterModule(new HermesModule(Configuration));
            })
            .ConfigureLogging(builder =>
            {
                builder.AddSerilog(Log.Logger, dispose: true);
            })
            .UseSerilog()
            .Build();
    }
}