using System.Reflection;
using Autofac;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Module = Autofac.Module;

namespace AlbyOnContainers.Hermes.IoC
{
    public class HermesModule : Module
    {
        readonly IConfiguration _configuration;

        public HermesModule(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.AddMassTransit(x =>
            {
                // add all consumers in the specified assembly
                x.AddConsumers(Assembly.GetExecutingAssembly());

                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host("localhost", config =>
                    {
                        config.Username("guest");
                        config.Password("guest");
                    });

                    cfg.ConfigureEndpoints(context);
                    cfg.Durable = true;
                    cfg.PrefetchCount = 5;
                    cfg.ExchangeType = "fanout";
                });
            });

            builder
                .RegisterAssemblyTypes(Assembly.GetExecutingAssembly())
                .AsImplementedInterfaces();
        }
    }
}