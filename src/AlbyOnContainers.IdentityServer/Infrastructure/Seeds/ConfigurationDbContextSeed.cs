using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Mappers;

namespace IdentityServer.Infrastructure.Seeds
{
    public static class ConfigurationDbContextSeed
    {
        public static async Task SeedAsync(ConfigurationDbContext context)
        {
            if (!context.Clients.Any())
            {
                foreach (var client in Config.Config.GetClients()) await context.Clients.AddAsync(client.ToEntity());
                await context.SaveChangesAsync();
            }

            if (!context.IdentityResources.Any())
            {
                foreach (var resource in Config.Config.GetResources()) await context.IdentityResources.AddAsync(resource.ToEntity());
                await context.SaveChangesAsync();
            }

            if (!context.ApiResources.Any())
            {
                foreach (var api in Config.Config.GetApis()) await context.ApiResources.AddAsync(api.ToEntity());
                await context.SaveChangesAsync();
            }
            
            if (!context.ApiScopes.Any())
            {
                foreach (var apiScope in Config.Config.GetApiScopes()) await context.ApiScopes.AddAsync(apiScope.ToEntity());
                await context.SaveChangesAsync();
            }
        }
    }
}