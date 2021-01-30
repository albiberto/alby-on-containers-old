using System.Collections.Generic;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using AlbyOnContainers.Messages;
using IdentityServer.Abstract;
using IdentityServer.Models;
using IdentityServer.Options;
using IdentityServer.Requests;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;

namespace IdentityServer.Handlers
{
    public class RegisterHandler : IRequestHandler<AccountRequests.Register, IResult<Unit, IdentityError>>
    {
        private readonly EmailOptions _options;

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMessagePublisher _publishMessage;

        public RegisterHandler(UserManager<ApplicationUser> userManager, IMessagePublisher publishMessage, IOptions<EmailOptions> options)
        {
            _userManager = userManager;
            _publishMessage = publishMessage;
            _options = options.Value;
        }

        public async Task<IResult<Unit, IdentityError>> Handle(AccountRequests.Register request, CancellationToken cancellationToken)
        {
            var user = new ApplicationUser
            {
                UserName = request.Email,
                Email = request.Email
            };

            var result = await _userManager.CreateAsync(user, request.Password);

            if (!result.Succeeded) return Result<IdentityError>.Value(Unit.Value);

            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

            var param = new Dictionary<string, string> {{"code", code}, {"userId", user.Id}, {"returnUrl", request.ReturnUrl}};

            var callbackUrl = QueryHelpers.AddQueryString(request.Host, param);

            var message = new EmailMessage
            {
                Sender = new MailAddress {Email = _options.Email, Name = _options.Name},
                Subject = "Confirm your email",
                Body = $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.",
                To = new[] {new MailAddress {Name = user.UserName, Email = user.Email}}
            };

            await _publishMessage.Send(cancellationToken, message); 

            return result.Succeeded
                ? Result<Unit>.Errors(result.Errors)
                : Result<IdentityError>.Value(Unit.Value);
        }
    }
}