using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.SystemTextJson;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AlbyOnContainers.Tools.Catalog
{
    public class Program
    {
        private static HttpMessageHandler BuildHttpMessageHandler(IServiceProvider provider) =>
            provider.GetService<AuthorizationMessageHandler>()?.ConfigureHandler(authorizedUrls: new[] {"https://localhost:5001"}, scopes: new[] {"companyApi"});

        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");

            builder.Services.AddScoped(sp => new GraphQLHttpClient( new GraphQLHttpClientOptions
            {
                EndPoint =  new Uri("http://localhost:5004/graphql"),
                PreprocessRequest = async (request, client) =>
                {
                    var provider = sp.GetRequiredService<IAccessTokenProvider>();
                    var tokenResult = await provider.RequestAccessToken();

                    var result = tokenResult.TryGetToken(out var token);

                    if(result) client.HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Value);
                    
                    return new GraphQLHttpRequest(request);
                }
            }, new SystemTextJsonSerializer()));

            builder.Services.AddOidcAuthentication(options =>
            {
                // Configure your authentication provider options here.
                // For more information, see https://aka.ms/blazor-standalone-auth
                builder.Configuration.Bind("Local", options.ProviderOptions);
            });

            await builder.Build().RunAsync();
        }
    }
}