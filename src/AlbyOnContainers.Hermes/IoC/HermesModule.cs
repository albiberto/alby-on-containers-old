using System.Reflection;
using Autofac;
using Hermes.Options;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Module = Autofac.Module;

namespace Hermes.IoC
{
    public class HermesModule : Module
    {
        readonly RabbitMQConfiguration _configuration;

        public HermesModule(RabbitMQConfiguration configuration)
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
                    cfg.Host(_configuration.Host, config =>
                    {
                        config.Username(_configuration.Username);
                        config.Password(_configuration.Password);
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