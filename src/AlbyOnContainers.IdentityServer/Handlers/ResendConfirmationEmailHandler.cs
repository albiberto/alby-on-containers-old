using System.Threading;
using System.Threading.Tasks;
using IdentityServer.Exceptions;
using IdentityServer.Models;
using IdentityServer.Publishers;
using IdentityServer.Requests;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace IdentityServer.Handlers
{
    public class ResendConfirmationEmailHandler : INotificationHandler<AccountRequests.ResendConfirmationEmail>
    {
        readonly UserManager<ApplicationUser> _userManager;
        readonly IEmailPublisher2 _publisher2;

        public ResendConfirmationEmailHandler(UserManager<ApplicationUser> userManager, IEmailPublisher2 publisher2)
        {
            _userManager = userManager;
            _publisher2 = publisher2;
        }

        public async Task Handle(AccountRequests.ResendConfirmationEmail request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);

            if (string.IsNullOrEmpty(user.Id)) throw new AuthenticationExceptions.EmailNotFound("Email not found");

            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            await _publisher2.SendConfirmationEmailAsync(user.Id, code, request.Host, request.ReturnUrl, user.UserName, user.Email, cancellationToken);
        }
    }
}