using System.Text;
using System.Threading;
using System.Threading.Tasks;
using IdentityServer.Models;
using IdentityServer.Requests;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;

namespace IdentityServer.Handlers
{
    public class ConfirmEmailHandler : IRequestHandler<AccountRequests.ConfirmEmail, IResult<Unit, Unit>>
    {
        readonly UserManager<ApplicationUser> _userManager;

        public ConfirmEmailHandler(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IResult<Unit, Unit>> Handle(AccountRequests.ConfirmEmail request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByIdAsync($"{request.UserId}");

            if (user == null) return Result<Unit>.Error(Unit.Value);

            var code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(request.Code));

            var result = await _userManager.ConfirmEmailAsync(user, code);

            return result.Succeeded
                ? Result<Unit>.Value(Unit.Value)
                : Result<Unit>.Error(Unit.Value);
        }
    }
}