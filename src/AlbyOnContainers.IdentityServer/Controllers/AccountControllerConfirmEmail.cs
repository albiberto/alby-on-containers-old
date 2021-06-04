using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;

namespace IdentityServer.Controllers
{
    public partial class AccountController
    {
        [HttpGet]
        public async Task<IActionResult> ConfirmEmail(string? userId, string? code, string? returnUrl = default)
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