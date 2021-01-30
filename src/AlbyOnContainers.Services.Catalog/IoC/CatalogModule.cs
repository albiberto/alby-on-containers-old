using Autofac;
using Catalog.Inputs;
using Catalog.Types;

namespace Catalog.IoC
{
    public class CatalogModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<Schema>().SingleInstance();
            builder.RegisterType<Query>().SingleInstance();
            builder.RegisterType<Mutation>().SingleInstance();

            builder.RegisterType<ProductType>().SingleInstance();
            builder.RegisterType<ProductInputType>().SingleInstance();

            builder.RegisterType<CategoryType>().SingleInstance();
            builder.RegisterType<CategoryInputType>().SingleInstance();

            builder.RegisterType<AttributeType>().SingleInstance();
            builder.RegisterType<AttributeInputType>().SingleInstance();

            builder.RegisterType<AttributeDescriptionType>().SingleInstance();
            builder.RegisterType<AttributeDescriptionInputType>().SingleInstance();
        }
    }
}