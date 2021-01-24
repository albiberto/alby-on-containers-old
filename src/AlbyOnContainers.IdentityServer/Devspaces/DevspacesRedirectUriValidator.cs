using System.Threading.Tasks;
using IdentityServer4.Models;
using IdentityServer4.Validation;
using Microsoft.Extensions.Logging;

namespace IdentityServer.Devspaces
{
    public class DevspacesRedirectUriValidator : IRedirectUriValidator
    {
        readonly ILogger<DevspacesRedirectUriValidator> _logger;

        public DevspacesRedirectUriValidator(ILogger<DevspacesRedirectUriValidator> logger)
        {
            _logger = logger;
        }

        public Task<bool> IsPostLogoutRedirectUriValidAsync(string requestedUri, Client client)
        {
            _logger.LogInformation("Client {ClientName} used post logout uri {RequestedUri}.", client.ClientName, requestedUri);
            return Task.FromResult(true);
        }

        public Task<bool> IsRedirectUriValidAsync(string requestedUri, Client client)
        {
            _logger.LogInformation("Client {ClientName} used post logout uri {RequestedUri}.", client.ClientName, requestedUri);
            return Task.FromResult(true);
        }
    }
}