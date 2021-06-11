using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using IdentityModel;
using Microsoft.AspNetCore.Authentication;

namespace IdentityServer.Areas.Diagnostics.Models
{
    public record DiagnosticsViewModel
    {
            public DiagnosticsViewModel(AuthenticateResult result)
            {
                AuthenticateResult = result;

                if (!(result.Properties?.Items.ContainsKey("client_list") ?? false)) return;

                var encoded = result.Properties.Items["client_list"];
                var bytes = Base64Url.Decode(encoded);
                var value = Encoding.UTF8.GetString(bytes);

                Clients = JsonSerializer.Deserialize<string[]>(value)?.ToHashSet() ?? new  HashSet<string>();
            }

            public AuthenticateResult AuthenticateResult { get; }
            public IReadOnlyCollection<string> Clients { get; } = new HashSet<string>();
    }
}