using System.Collections.Generic;
using IdentityModel;
using IdentityServer.Resources;
using IdentityServer4;
using IdentityServer4.Models;

namespace IdentityServer.Infrastructure.Seeds.Config
{
    public static class Config
    {
        // ApiResources define the apis in your system
        public static IEnumerable<ApiResource> GetApis()
        {
            var resource1 = new ApiResource("productapi", "The ProductApi Resource", new[] {JwtClaimTypes.Role});
            var resource2 = new ApiResource("catalogapi", "The CatalogApi Resource", new[] {JwtClaimTypes.Role});
            var resource3 = new ApiResource("kharonte", "The PhotoGateway Resource", new[] {JwtClaimTypes.Role});

            resource1.Scopes = new List<string> {"products"};
            resource2.Scopes = new List<string> {"products"};
            resource3.Scopes = new List<string> {"photos"};

            return new List<ApiResource> {resource1, resource2, resource3};
        }

        public static IEnumerable<ApiScope> GetApiScopes()
        {
            return new List<ApiScope>
            {
                new(name: "photos", displayName: "The Photo Scopes"),
                new(name: "products", displayName: "The Product Scopes")
            };
        }
        
        // Identity resources are data like user ID, name, or email address of a user
        // see: http://docs.identityserver.io/en/release/configuration/resources.html
        public static IEnumerable<IdentityResource> GetResources() =>
            new List<IdentityResource>
            {
                new IdentityResources.OpenId(),
                new ProfileWithRoleIdentityResource(),
                new IdentityResources.Email(),
                new IdentityResources.Address()
            };

        // client want to access resources (aka scopes)
        public static IEnumerable<Client> GetClients() =>
            new List<Client>
            {
                new()
                {
                    ClientId = "Catalog",
                    ClientName = "Wasm Client",
                    AllowedGrantTypes = GrantTypes.Code,

                    RequireClientSecret = false,
                    RequirePkce = true,
                    RequireConsent = true,

                    RedirectUris =           { "http://localhost:5005/authentication/login-callback" },
                    PostLogoutRedirectUris = { "http://localhost:5005" },
                    AllowedCorsOrigins =     { "http://localhost:5005" },

                    AllowedScopes =
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        "products"
                    },
                },

                new()
                {
                    ClientId = "Blazor",
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