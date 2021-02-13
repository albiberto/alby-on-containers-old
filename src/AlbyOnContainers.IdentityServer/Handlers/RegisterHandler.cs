using System.Threading;
using System.Threading.Tasks;
using IdentityServer.Models;
using IdentityServer.Publishers;
using IdentityServer.Requests;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace IdentityServer.Handlers
{
    public class RegisterHandler : IRequestHandler<AccountRequests.Register, IResult<Unit, IdentityError>>
    {
        readonly UserManager<ApplicationUser> _userManager;
        readonly IEmailPublisher2 _publisher2;

        public RegisterHandler(UserManager<ApplicationUser> userManager, IEmailPublisher2 publisher2)
        {
            _userManager = userManager;
            _publisher2 = publisher2;
        }

        public async Task<IResult<Unit, IdentityError>> Handle(AccountRequests.Register request, CancellationToken cancellationToken)
        {
            var user = new ApplicationUser
            {
                UserName = request.Username ?? request.Email,
                Email = request.Email
            };

            var result = await _userManager.CreateAsync(user, request.Password);

            if (!result.Succeeded) return Result<IdentityError>.Value(Unit.Value);
            
            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            await _publisher2.SendConfirmationEmailAsync(user.Id, code, request.Host, request.ReturnUrl, request.Email, request.Email, cancellationToken);

            return Result<IdentityError>.Value(Unit.Value);
        }
    }
}