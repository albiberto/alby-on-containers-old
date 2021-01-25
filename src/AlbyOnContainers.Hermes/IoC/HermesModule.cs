using System;
using System.Linq;
using System.Net.Http;
using System.Net.Sockets;
using System.Reflection;
using Autofac;
using GreenPipes;
using GreenPipes.Configurators;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Serilog;
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
                    Log.Logger.Information($"RabbitMQ {{Host: {_configuration["RabbitMQ:Host"]} User: {_configuration["RabbitMQ:Username"]} Pass: {_configuration["RabbitMQ:Password"]}}}");
                    
                    cfg.Host(_configuration["RabbitMQ:Host"], config =>
                    {
                        config.Username(_configuration["RabbitMQ:Username"]);
                        config.Password(_configuration["RabbitMQ:Password"]);
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