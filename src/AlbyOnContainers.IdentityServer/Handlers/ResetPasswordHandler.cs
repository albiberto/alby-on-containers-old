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
    public class ResetPasswordHandler : INotificationHandler<AccountRequests.ResetPassword>
    {
        readonly UserManager<ApplicationUser> _userManager;

        public ResetPasswordHandler(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task Handle(AccountRequests.ResetPassword request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);

            // Don't reveal that the user does not exist
            if (user == default) throw new AuthenticationExceptions.UserNotFound($"User not found!");             

            var code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(request.Code));

            var result = await _userManager.ResetPasswordAsync(user, code, request.Password);

            // Don't reveal that the user is not confirmed
            if (!result.Succeeded) throw new AuthenticationExceptions.Generic("Cannot reset password");
        }
    }
}