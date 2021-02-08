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
        readonly IEmailPublisher _publisher;

        public ResendConfirmationEmailHandler(UserManager<ApplicationUser> userManager, IEmailPublisher publisher)
        {
            _userManager = userManager;
            _publisher = publisher;
        }

        public async Task Handle(AccountRequests.ResendConfirmationEmail request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);

            if (string.IsNullOrEmpty(user.Id)) throw new AuthenticationExceptions.EmailNotFound("Email not found");

            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            await _publisher.SendConfirmationEmailAsync(user.Id, code, request.Host, request.ReturnUrl, user.UserName, user.Email, cancellationToken);
        }
    }
}