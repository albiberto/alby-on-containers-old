using Demetra.Aggregates.Attr;
using Demetra.Aggregates.Products;
using Demetra.DataLoader;
using Demetra.Infrastructure;
using Demetra.Types;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Demetra
{
    public class Startup
    {
        readonly IWebHostEnvironment _env;

        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            _env = env;
            Configuration = configuration;
        }

        IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            var connection = Configuration.GetConnectionString("DefaultDatabase");
            services.AddPooledDbContextFactory<ApplicationDbContext>(optionsBuilder => optionsBuilder.UseNpgsql(connection));
            
            services
                .AddGraphQLServer()
                .AddQueryType(d => d.Name("Query"))
                .AddTypeExtension<ProductQueries>()
                .AddTypeExtension<AttrQueries>()
                .AddMutationType(d => d.Name("Mutation"))
                .AddTypeExtension<ProductMutations>()
                .EnableRelaySupport()
                .AddType<ProductType>()
                .AddType<CategoryType>()
                .AddType<AttrType>()
                .AddType<AttrDescrType>()
                .AddTypeExtension<ProductMutations>()
                .AddDataLoader<ProductByIdDataLoader>()
                .AddDataLoader<CategoryByIdDataLoader>()
                .AddDataLoader<AttrByIdDataLoader>()
                .AddDataLoader<AttrDescrByIdDataLoader>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();
            
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGraphQL();
            });
        }
    }
}