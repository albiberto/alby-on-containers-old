using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using AlbyOnContainers.Messages;
using IdentityServer.Models;
using IdentityServer.Requests;
using MassTransit;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;

namespace IdentityServer.Handlers
{
    public class RegisterHandler : IRequestHandler<AccountRequests.Register, IResult<Unit, IdentityError>>
    {
        readonly Options _options;
        readonly IPublishEndpoint _publishEndpoint;
        readonly ILogger<RegisterHandler> _logger;

        readonly UserManager<ApplicationUser> _userManager;

        public RegisterHandler(UserManager<ApplicationUser> userManager, IPublishEndpoint publishEndpoint, ILogger<RegisterHandler> logger, IOptions<Options> options)
        {
            _userManager = userManager;
            _publishEndpoint = publishEndpoint;
            _logger = logger;
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
            
            const int retries = 3;
            var retry = Policy.Handle<Exception>()
                .WaitAndRetryAsync(retries,
                    retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    (exception, _, r, _) => _logger.LogWarning(exception, "[{prefix}] Exception {ExceptionType} with message {Message} detected on attempt {retry} of {retries}", nameof(Program), exception.GetType().Name,
                        exception.Message, r, retries)
                );
            
            await retry.ExecuteAsync(async () => await  _publishEndpoint.Publish(message, cancellationToken));

            return result.Succeeded
                ? Result<Unit>.Errors(result.Errors)
                : Result<IdentityError>.Value(Unit.Value);
        }

        public class Options
        {
            public string Email { get; set; }
            public string Name { get; set; }
        }
    }
}