using System.Reflection;
using Autofac;
using Catalog.Inputs;
using Catalog.Models;
using Catalog.Repository;
using Catalog.Types;
using GraphQL.Types;
using Module = Autofac.Module;

namespace Catalog.IoC
{
    public class CatalogModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<Schema>().SingleInstance();

            builder.RegisterAssemblyTypes(Assembly.GetAssembly(typeof(IAggregateRoot))!)
                .AssignableTo<IAggregateRoot>()
                .AsImplementedInterfaces()
                .InstancePerLifetimeScope();

            builder.RegisterAssemblyTypes(Assembly.GetAssembly(typeof(IRepository<>))!)
                .AsClosedTypesOf(typeof(IRepository<>))
                .AsImplementedInterfaces()
                .InstancePerLifetimeScope();

            builder.RegisterAssemblyTypes(Assembly.GetAssembly(typeof(ObjectGraphType<>))!)
                .AsClosedTypesOf(typeof(ObjectGraphType<>))
                .AsSelf()
                .SingleInstance();
            
            builder.RegisterAssemblyTypes(Assembly.GetAssembly(typeof(InputObjectGraphType<>))!)
                .AsClosedTypesOf(typeof(InputObjectGraphType<>))
                .AsSelf()
                .SingleInstance();
        }
    }
}