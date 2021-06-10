using IdentityServer.Models;
using IdentityServer.Options;
using IdentityServer.Publishers;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace IdentityServer.Controllers
{
    public partial class ManageController : Controller
    {
        readonly IEmailPublisher _email;
        readonly EmailOptions _options;
        readonly SignInManager<ApplicationUser> _signInManager;
        readonly UserManager<ApplicationUser> _userManager;

        public ManageController(IEmailPublisher email, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IOptions<EmailOptions> options)
        {
            _email = email;
            _userManager = userManager;
            _signInManager = signInManager;
            _options = options.Value;
        }
    }
}