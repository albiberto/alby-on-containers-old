using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer.Areas.Diagnostics.Models;
using Microsoft.AspNetCore.Authentication;

namespace IdentityServer.Areas.Diagnostics.Controllers

{
    [Area("Diagnostics")]
    public class DiagnosticsController : Controller
    {
        public async Task<IActionResult> Index()
        {
            var localAddresses = new [] { "127.0.0.1", "::1", $"{HttpContext.Connection.LocalIpAddress}" };
            if (!localAddresses.Contains($"{HttpContext.Connection.RemoteIpAddress}"))
            {
                return NotFound();
            }

            var model = new DiagnosticsViewModel(await HttpContext.AuthenticateAsync());
            return View(model);
        }
    }
}