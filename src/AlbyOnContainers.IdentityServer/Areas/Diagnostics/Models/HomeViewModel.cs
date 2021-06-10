using System.Collections.Generic;
using System.Text;
using IdentityModel;
using Microsoft.AspNetCore.Authentication;
using Newtonsoft.Json;

namespace IdentityServer.Areas.Diagnostics.Models
{
    public class HomeViewModel
    {
            public HomeViewModel(AuthenticateResult result)
            {
                AuthenticateResult = result;

                if (!(result.Properties?.Items.ContainsKey("client_list") ?? false)) return;
                
                var encoded = result.Properties.Items["client_list"];
                var bytes = Base64Url.Decode(encoded);
                var value = Encoding.UTF8.GetString(bytes);

                Clients = JsonConvert.DeserializeObject<string[]>(value);
            }

            public AuthenticateResult AuthenticateResult { get; }
            public IReadOnlyCollection<string> Clients { get; } = new List<string>();
    }
}