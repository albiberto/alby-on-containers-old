using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace IdentityServer.IoC
{
    // ReSharper disable once InconsistentNaming
    public static class IServiceCollectionSecurityExtensions
    {
        // https: //pradeeploganathan.com/aspnetcore/https-in-asp-net-core-31/

        public static void AddReverseProxy(this IServiceCollection services)
        {
            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
                options.KnownNetworks.Clear();
                options.KnownProxies.Clear();
            });
        }

        public static void AddHttpsRedirection(this IServiceCollection services, IWebHostEnvironment env)
        {
            var result = int.TryParse(Environment.GetEnvironmentVariable("ASPNETCORE_HTTPS_PORT"), out var port);
            if (!result) port = 443;

            if (env.IsDevelopment() || env.IsEnvironment("Debug"))
                services.AddHttpsRedirection(opts =>
                {
                    opts.RedirectStatusCode = StatusCodes.Status307TemporaryRedirect;
                    opts.HttpsPort = port;
                });
            else
                services.AddHttpsRedirection(opts =>
                {
                    opts.RedirectStatusCode = StatusCodes.Status308PermanentRedirect;
                    opts.HttpsPort = port;
                });
        }

        public static void AddHsts(this IServiceCollection services)
        {
            services.AddHsts(options =>
            {
                options.Preload = true;
                options.IncludeSubDomains = true;
                options.MaxAge = TimeSpan.FromDays(15);
            });
        }
    }
}