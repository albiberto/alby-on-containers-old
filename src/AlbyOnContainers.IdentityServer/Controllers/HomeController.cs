using System.Threading.Tasks;
using IdentityServer.ViewModels;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;

namespace IdentityServer.Controllers
{
    [AllowAnonymous]
    public class HomeController : Controller
    {
        readonly IIdentityServerInteractionService _interaction;
        readonly IWebHostEnvironment _environment;

        public HomeController(IIdentityServerInteractionService interaction, IWebHostEnvironment environment)
        {
            _interaction = interaction;
            _environment = environment;
        }

        [HttpGet]
        public IActionResult Index() => View();

        [HttpGet]
        public async Task<IActionResult> Error(string errorId)
        {
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