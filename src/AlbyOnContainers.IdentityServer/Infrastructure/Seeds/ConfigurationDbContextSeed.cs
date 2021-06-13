using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Mappers;
using Libraries.IHostExtensions;

namespace IdentityServer.Infrastructure.Seeds
{
    public class ConfigurationDbContextSeed : IDbContextSeed<ConfigurationDbContext>
    {
        readonly ConfigurationDbContext _context;

        public ConfigurationDbContextSeed(ConfigurationDbContext context)
        {
            _context = context;
        }

        public async Task SeedAsync()
        {
            if (!_context.Clients.Any())
            {
                foreach (var client in Config.Config.GetClients()) await _context.Clients.AddAsync(client.ToEntity());
                await _context.SaveChangesAsync();
            }

            if (!_context.IdentityResources.Any())
            {
                foreach (var resource in Config.Config.GetResources()) await _context.IdentityResources.AddAsync(resource.ToEntity());
                await _context.SaveChangesAsync();
            }

            if (!_context.ApiResources.Any())
            {
                foreach (var api in Config.Config.GetApis()) await _context.ApiResources.AddAsync(api.ToEntity());
                await _context.SaveChangesAsync();
            }
            
            if (!_context.ApiScopes.Any())
            {
                foreach (var apiScope in Config.Config.GetApiScopes()) await _context.ApiScopes.AddAsync(apiScope.ToEntity());
                await _context.SaveChangesAsync();
            }
        }
    }
}