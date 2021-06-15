using System.Threading.Tasks;
using IdentityServer.ViewModels;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace IdentityServer.Controllers
{
    [AllowAnonymous]
    public class HomeController : Controller
    {
        readonly IIdentityServerInteractionService _interaction;
        readonly IWebHostEnvironment _environment;
        readonly ILogger _logger;

        public HomeController(IIdentityServerInteractionService interaction, IWebHostEnvironment environment, ILogger<HomeController> logger)
        {
            _interaction = interaction;
            _environment = environment;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Index() => View();

        [HttpGet]
        public async Task<IActionResult> Error(string errorId)
        {
            // retrieve error details from identityserver
            var message = await _interaction.GetErrorContextAsync(errorId);
            
            if (message == default) return View("Error", new ErrorViewModel());
            
            if (!_environment.IsDevelopment())
            {
                // only show in development
                message.ErrorDescription = null;
            }

            return View("Error", new ErrorViewModel(message));
        }
    }
}