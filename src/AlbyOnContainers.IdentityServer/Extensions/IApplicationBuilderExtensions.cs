using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;

namespace IdentityServer.Extensions
{
    // ReSharper disable once InconsistentNaming
    public static class IApplicationBuilderExtensions
    {
        public static void UseProxy(this IApplicationBuilder app)
        {
            app.Use(async (context, next) =>
            {
                var forwardedPath = context.Request.Headers["X-Forwarded-Path"].FirstOrDefault();
                if (!string.IsNullOrEmpty(forwardedPath)) context.Request.PathBase = forwardedPath;

                await next();
            });

            app.UseForwardedHeaders();
        }

        public static void UseSecurityHeaders(this IApplicationBuilder app)
        {
            app.Use(async (context, next) =>
            {
                context.Response.OnStarting(() =>
                {
                    // var result = context;
                    // if (result is not ViewResult) return;

                    // https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/X-Content-Type-Options
                    if (!context.Response.Headers.ContainsKey("X-Content-Type-Options")) context.Response.Headers.Add("X-Content-Type-Options", "nosniff");

                    // https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/X-Frame-Options
                    if (!context.Response.Headers.ContainsKey("X-Frame-Options")) context.Response.Headers.Add("X-Frame-Options", "SAMEORIGIN");

                    // https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Content-Security-Policy
                    const string csp = "default-src 'self'; object-src 'none'; frame-ancestors 'none'; sandbox allow-forms allow-same-origin allow-scripts; base-uri 'self'; upgrade-insecure-requests;";
                    // also consider adding upgrade-insecure-requests once you have HTTPS in place for production
                    // csp += "upgrade-insecure-requests;";
                    // also an example if you need client images to be displayed from twitter
                    // csp += "img-src 'self' https://pbs.twimg.com;";

                    // once for standards compliant browsers
                    if (!context.Response.Headers.ContainsKey("Content-Security-Policy")) context.Response.Headers.Add("Content-Security-Policy", csp);
                    // and once again for IE
                    if (!context.Response.Headers.ContainsKey("X-Content-Security-Policy")) context.Response.Headers.Add("X-Content-Security-Policy", csp);
                    //
                    // // https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Referrer-Policy
                    const string referrerPolicy = "no-referrer";
                    if (!context.Response.Headers.ContainsKey("Referrer-Policy")) context.Response.Headers.Add("Referrer-Policy", referrerPolicy);

                    return Task.FromResult(0);
                });

                await next();
            });
        }
    }
}