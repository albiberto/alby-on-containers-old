using System.Collections.Generic;
using IdentityModel;
using IdentityServer4;
using IdentityServer4.Models;

namespace IdentityServer.Infrastructure
{
    public static class Config
    {
        // ApiResources define the apis in your system
        public static IEnumerable<ApiResource> GetApis() =>
            new List<ApiResource>
            {
                new("products", "Products Service",  new[] { JwtClaimTypes.Role }),
                new("weatherapi", "The Weather API", new[] { JwtClaimTypes.Role })
            };

        public static IEnumerable<ApiScope> GetApiScopes()
        {
            return new List<ApiScope>
            {
                new ApiScope(name: "read",   displayName: "Read your data."),
                new ApiScope(name: "write",  displayName: "Write your data."),
                new ApiScope(name: "delete", displayName: "Delete your data."),
                new ApiScope(name: "weatherapi", displayName: "The Weather API")
            };
        }
        
        // Identity resources are data like user ID, name, or email address of a user
        // see: http://docs.identityserver.io/en/release/configuration/resources.html
        public static IEnumerable<IdentityResource> GetResources() =>
            new List<IdentityResource>
            {
                new IdentityResources.OpenId(),
                new ProfileWithRoleIdentityResource(),
                //new IdentityResources.Profile(),
                new IdentityResources.Email()
            };

        // client want to access resources (aka scopes)
        public static IEnumerable<Client> GetClients(Dictionary<string, string> clientsUrl) =>
            new List<Client>
            {
                // JavaScript Client
                new()
                {
                    ClientId = "js",
                    ClientName = "eShop SPA OpenId Client",
                    AllowedGrantTypes = GrantTypes.Implicit,
                    AllowAccessTokensViaBrowser = true,
                    RedirectUris = {$"{clientsUrl["Spa"]}/"},
                    RequireConsent = false,
                    PostLogoutRedirectUris = {$"{clientsUrl["Spa"]}/"},
                    AllowedCorsOrigins = {$"{clientsUrl["Spa"]}"},
                    AllowedScopes =
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        IdentityServerConstants.StandardScopes.Email,
                      "products"
                    }
                },

                new()
                {
                    ClientId = "blazor",
                    AllowedGrantTypes = GrantTypes.Code,
                    RequirePkce = true,
                    RequireClientSecret = false,
                    RequireConsent = true,
                    AllowedCorsOrigins = { "https://localhost:4000" },
                    RedirectUris = { "https://localhost:4000/authentication/login-callback" },
                    PostLogoutRedirectUris = { "https://localhost:4000/" },
                    Enabled = true,
                    AllowedScopes =
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        IdentityServerConstants.StandardScopes.Email,
                        "weatherapi"
                    }
                },
            };
    }
}