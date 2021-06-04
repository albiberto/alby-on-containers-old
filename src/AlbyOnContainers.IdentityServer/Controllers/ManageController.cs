using System.Threading.Tasks;
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
        private readonly IEmailPublisher _email;
        private readonly EmailOptions _options;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public ManageController(IEmailPublisher email, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IOptions<EmailOptions> options)
        {
            _email = email;
            _userManager = userManager;
            _signInManager = signInManager;
            _options = options.Value;
        }
    }
}