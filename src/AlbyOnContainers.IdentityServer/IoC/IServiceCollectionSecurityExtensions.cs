using System;
using IdentityServer4;
using Microsoft.AspNetCore.Authorization.Infrastructure;
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

        public static void AddAuthorizationPolicies(this IServiceCollection services) =>
            services.AddAuthorization(options =>
                {
                    options.AddPolicy("All", policy =>
                    {
                        policy.Requirements.Add(new RolesAuthorizationRequirement(new[] {"Admin", "User"}));
                    });
                    
                    options.AddPolicy("Admin", policy =>
                    {
                        policy.Requirements.Add(new RolesAuthorizationRequirement(new[] {"Admin"}));
                    });
                });

        public static void AddCorsPolicies(this IServiceCollection services) =>
            services.AddCors(options => 
                options.AddPolicy("AlbyPolicy",builder => 
                    builder.AllowAnyOrigin().WithMethods("GET", "POST").WithHeaders()
            ));

        public static void AddAuthenticationWithExternalProviders(this IServiceCollection services)
        {
            services.AddAuthentication()
                .AddGoogle(options =>
                {
                    options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;

                    options.ClientId = "516459164098-dm04ij4omekj0aii8gntjm5neujul5tn.apps.googleusercontent.com";
                    options.ClientSecret = "1PNiGZSk1hjENrxnFdTUyHKY";
                })
                .AddFacebook(options =>
                {
                    options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;

                    options.AppId = "191746359619007";
                    options.AppSecret = "78a544dede886fe61f885e558980e6d4";
                    options.AccessDeniedPath = "/AccessDeniedPathInfo";
                });
        }
    }
}