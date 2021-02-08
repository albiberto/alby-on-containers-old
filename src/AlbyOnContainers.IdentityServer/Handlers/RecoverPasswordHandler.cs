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
    public class RecoverPasswordHandler : INotificationHandler<AccountRequests.RecoverPassword>
    {
        readonly UserManager<ApplicationUser> _userManager;
        readonly IEmailPublisher _publisher;

        public RecoverPasswordHandler(UserManager<ApplicationUser> userManager, IEmailPublisher publisher)
        {
            _userManager = userManager;
            _publisher = publisher;
        }

        public async Task Handle(AccountRequests.RecoverPassword request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);

            if (user == default || !await _userManager.IsEmailConfirmedAsync(user)) throw new AuthenticationExceptions.Generic(); // Don't reveal that the user does not exist or is not confirmed

            var code = await _userManager.GeneratePasswordResetTokenAsync(user);
            await _publisher.SendRecoverPasswordEmailAsync(user.Id, code, request.Host, request.ReturnUrl, request.Email, request.Email, cancellationToken);
        }
    }
}