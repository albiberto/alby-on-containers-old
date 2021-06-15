using IdentityServer.Models;
using IdentityServer.Options;
using IdentityServer4.Services;
using IdentityServer4.Stores;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace IdentityServer.Controllers.Account
{
    [AllowAnonymous]
    public partial class AccountController : Controller
    {
        readonly AccountOptions _accountOptions;
        readonly IClientStore _clientStore;
        readonly IEventService _events;
        readonly IIdentityServerInteractionService _interaction;
        readonly IAuthenticationSchemeProvider _schemeProvider;
        readonly SignInManager<ApplicationUser> _signInManager;
        readonly UserManager<ApplicationUser> _userManager;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IIdentityServerInteractionService interaction,
            IClientStore clientStore,
            IAuthenticationSchemeProvider schemeProvider,
            IEventService events,
            IOptions<ControllersOptions> accountOptions)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _interaction = interaction;
            _clientStore = clientStore;
            _schemeProvider = schemeProvider;
            _events = events;
            _accountOptions = accountOptions.Value.Account ?? new AccountOptions();
        }

        [HttpGet]
        public IActionResult AccessDenied() => View();
    }
}