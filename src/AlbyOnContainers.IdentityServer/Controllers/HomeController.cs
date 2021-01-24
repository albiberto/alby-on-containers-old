using System.Threading.Tasks;
using IdentityServer.Models;
using IdentityServer.Services;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Mvc;

namespace IdentityServer.Controllers
{
    public class HomeController : Controller
    {
        readonly IIdentityServerInteractionService _interaction;
        readonly IRedirectService _redirectSvc;

        public HomeController(IIdentityServerInteractionService interaction, IRedirectService redirectSvc)
        {
            _interaction = interaction;
            _redirectSvc = redirectSvc;
        }

        public IActionResult Index(string returnUrl) => View();

        public IActionResult ReturnToOriginalApplication(string returnUrl)
        {
            if (returnUrl != null)
                return Redirect(_redirectSvc.ExtractRedirectUriFromReturnUrl(returnUrl));
            return RedirectToAction("Index", "Home");
        }

        /// <summary>
        ///     Shows the error page
        /// </summary>
        public async Task<IActionResult> Error(string errorId)
        {
            var vm = new ErrorViewModel();

            // retrieve error details from identityserver
            var message = await _interaction.GetErrorContextAsync(errorId);
            if (message != null) vm.Error = message;

            return View("Error", vm);
        }
    }
}