using System.Text;
using System.Threading.Tasks;
using IdentityServer.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;

namespace IdentityServer.Controllers
{
    public class ConfirmEmailController : Controller
    {
        readonly UserManager<ApplicationUser> _userManager;

        public ConfirmEmailController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }
        
        [HttpGet]
        public async Task<IActionResult> Index(string? userId, string? code, string? returnUrl = default)
        {
            ViewData["ReturnRul"] = returnUrl;

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(code)) return Redirect("~/");

            var user = await _userManager.FindByIdAsync(userId);
            if (user == default) return NotFound($"Unable to load user with ID '{userId}'.");

            code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
            
            var result = await _userManager.ConfirmEmailAsync(user, code);

            ViewData["StatusMessage"] = result.Succeeded
                ? "Grazie per aver confermato la tua email."
                : "Ops... si e' verificato un errore durante la consegna della mail.";
            
            return View();
        }
    }
}