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
    public class ResetPasswordHandler : IRequestHandler<AccountRequests.PostResetPassword, IResult<Unit, IdentityError>>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        
        public ResetPasswordHandler(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }
        
        public async Task<IResult<Unit, IdentityError>> Handle(AccountRequests.PostResetPassword request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            
            if (user == null)
            {
                // Don't reveal that the user does not exist or is not confirmed
                return Result<Unit>.Error(new IdentityError());
            }
            
            var code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(request.Code));
            
            var result = await _userManager.ResetPasswordAsync(user, code, request.Password);
            
            return  Result<IdentityError>.Value(Unit.Value);
        }
    }
}