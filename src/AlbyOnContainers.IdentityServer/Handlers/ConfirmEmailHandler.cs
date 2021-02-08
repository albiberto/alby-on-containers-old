using System.Text;
using System.Threading;
using System.Threading.Tasks;
using IdentityServer.Exceptions;
using IdentityServer.Models;
using IdentityServer.Requests;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;

namespace IdentityServer.Handlers
{
    public class ConfirmEmailHandler : INotificationHandler<AccountRequests.ConfirmEmail>
    {
        readonly UserManager<ApplicationUser> _userManager;

        public ConfirmEmailHandler(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task Handle(AccountRequests.ConfirmEmail request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByIdAsync($"{request.UserId}");

            if (user == default) throw new AuthenticationExceptions.Generic("Id not found");

            var code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(request.Code));
            var result = await _userManager.ConfirmEmailAsync(user, code);

            if (!result.Succeeded) throw new AuthenticationExceptions.Generic("Cannot confirm email");
        }
    }
}