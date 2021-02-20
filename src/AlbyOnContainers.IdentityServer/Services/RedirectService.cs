using System.Net;
using System.Text.RegularExpressions;

namespace IdentityServer.Services
{
    public interface IRedirectService
    {
        string ExtractRedirectUriFromReturnUrl(string url);
    }

    public class RedirectService : IRedirectService
    {
        public string ExtractRedirectUriFromReturnUrl(string url)
        {
            var decodedUrl = WebUtility.HtmlDecode(url);

            var results = Regex.Split(decodedUrl, "redirect_uri=");

            if (results.Length < 2) return string.Empty;

            var result = results[1];

            var splitKey = result.Contains("signin-oidc") ? "signin-oidc" : "scope";

            results = Regex.Split(result, splitKey);
            if (results.Length < 2) return string.Empty;

            result = results[0];

            return result.Replace("%3A", ":").Replace("%2F", "/").Replace("&", "");
        }
    }
}