using System;
using System.Threading;
using System.Threading.Tasks;
using IdentityServer.Models;
using IdentityServer.Options;
using IdentityServer.Requests;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace IdentityServer.Handlers
{
    public class LoginHandler : IRequestHandler<AccountRequests.Login, IResult<Unit, Unit>>
    {
        readonly TokenLifetimeOptions _options;
        readonly SignInManager<ApplicationUser> _signInManager;
        readonly UserManager<ApplicationUser> _userManager;

        public LoginHandler(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager, IOptions<TokenLifetimeOptions> options)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _options = options.Value;
        }

        public async Task<IResult<Unit, Unit>> Handle(AccountRequests.Login request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);

            if (!await _userManager.CheckPasswordAsync(user, request.Password)) return Result<Unit>.Error(Unit.Value);

            var props = new AuthenticationProperties
            {
                ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(_options.Minutes),
                AllowRefresh = true,
                RedirectUri = request.ReturnUrl
            };

            if (request.RememberMe)
            {
                props.ExpiresUtc = DateTimeOffset.UtcNow.AddDays(_options.Days);
                props.IsPersistent = true;
            }

            await _signInManager.SignInAsync(user, props);

            return Result<Unit>.Value(Unit.Value);
        }
    }
}