using Microsoft.Extensions.DependencyInjection;

namespace IdentityServer.Devspaces
{
    internal static class IdentityDevspacesBuilderExtensions
    {
        public static IIdentityServerBuilder AddDevspacesIfNeeded(this IIdentityServerBuilder builder, bool useDevspaces)
        {
            if (useDevspaces) builder.AddRedirectUriValidator<DevspacesRedirectUriValidator>();
            return builder;
        }
    }
}