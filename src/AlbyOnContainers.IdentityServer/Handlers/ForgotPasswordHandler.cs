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
    public class ForgotPasswordHandler : IRequestHandler<AccountRequests.ForgotPassword, IResult<Unit, IdentityError>>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMessagePublisher _publishMessage;
        private readonly EmailOptions _options;

        public ForgotPasswordHandler(UserManager<ApplicationUser> userManager, IMessagePublisher publishMessage, IOptions<EmailOptions> options)
        {
            _userManager = userManager;
            _publishMessage = publishMessage;
            _options = options.Value;
        }
        
        public async Task<IResult<Unit, IdentityError>> Handle(AccountRequests.ForgotPassword request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            
            if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
            {
                // Don't reveal that the user does not exist or is not confirmed
                return Result<Unit>.Error(new IdentityError());
            }
            
            // For more information on how to enable account confirmation and password reset please 
            // visit https://go.microsoft.com/fwlink/?LinkID=532713
            var code = await _userManager.GeneratePasswordResetTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            
            var param = new Dictionary<string, string> {{"code", code}, {"userId", user.Id}, {"returnUrl", request.ReturnUrl}};

            var callbackUrl = QueryHelpers.AddQueryString(request.Host, param);
            
            var message = new EmailMessage
            {
                Sender = new MailAddress { Email = _options.Email, Name = _options.Name },
                Subject = "Reset Password",
                Body =  $"Please reset your password by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.",
                To = new[] { new MailAddress { Name = user.UserName, Email = user.Email } }
            };
            
            await _publishMessage.Send(cancellationToken, message); 

            return  Result<IdentityError>.Value(Unit.Value);
        }
    }
}