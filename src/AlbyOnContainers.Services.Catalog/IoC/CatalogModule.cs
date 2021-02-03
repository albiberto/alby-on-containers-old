using System.Reflection;
using Autofac;
using Catalog.Models;
using Catalog.Repository;
using GraphQL.Types;

namespace Catalog.IoC
{
    public class CatalogModule : Autofac.Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<Schema>().SingleInstance();

            builder.RegisterAssemblyTypes(Assembly.GetAssembly(typeof(Program))!)
                .AssignableTo<IAggregateRoot>()
                .AsImplementedInterfaces()
                .InstancePerLifetimeScope();

            builder.RegisterAssemblyTypes(Assembly.GetAssembly(typeof(Program))!)
                .AsClosedTypesOf(typeof(IRepository<>))
                .AsImplementedInterfaces()
                .InstancePerLifetimeScope();

            builder.RegisterAssemblyTypes(Assembly.GetAssembly(typeof(Program))!)
                .AsClosedTypesOf(typeof(ObjectGraphType<>))
                .AsSelf()
                .SingleInstance();

            builder.RegisterAssemblyTypes(Assembly.GetAssembly(typeof(Program))!)
                .AsClosedTypesOf(typeof(InputObjectGraphType<>))
                .AsSelf()
                .SingleInstance();
        }
    }
}