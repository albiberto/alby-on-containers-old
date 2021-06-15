using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityModel;
using IdentityServer.Extensions;
using IdentityServer.Models;
using IdentityServer4;
using IdentityServer4.Events;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace IdentityServer.Controllers.External
{
    [AllowAnonymous]
    public class ExternalController : Controller
    {
        readonly IEventService _events;
        readonly IIdentityServerInteractionService _interaction;
        readonly SignInManager<ApplicationUser> _signInManager;
        readonly UserManager<ApplicationUser> _userManager;

        public ExternalController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IIdentityServerInteractionService interaction,
            IEventService events)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _interaction = interaction;
            _events = events;
        }
        
        [HttpGet]
        public IActionResult Challenge(string scheme, string? returnUrl = default)
        {
            returnUrl ??= "~/";

            // validate returnUrl - either it is a valid OIDC URL or back to a local page
            if (!Url.IsLocalUrl(returnUrl) && !_interaction.IsValidReturnUrl(returnUrl))
                // user might have clicked on a malicious link - should be logged
                throw new Exception("invalid return URL");

            // start challenge and roundtrip the return URL and scheme 
            var props = new AuthenticationProperties
            {
                RedirectUri = Url.Action(nameof(Callback)),
                Items =
                {
                    {"returnUrl", returnUrl},
                    {"scheme", scheme}
                }
            };

            return Challenge(props, scheme);
        }

        [HttpGet]
        public async Task<IActionResult> Callback()
        {
            // read external identity from the temporary cookie
            var result = await HttpContext.AuthenticateAsync(IdentityServerConstants.ExternalCookieAuthenticationScheme);
            if (!result?.Succeeded ?? true) throw new Exception("External authentication error");

            // lookup our user and external provider info
            var (user, provider, providerUserId, claims) = await FindUserFromExternalProviderAsync(result);
            user ??= await AutoProvisionUserAsync(provider, providerUserId, claims);

            // this allows us to collect any additional claims or properties
            // for the specific protocols used and store them in the local auth cookie.
            // this is typically used to store data needed for signout from those protocols.
            var additionalLocalClaims = new List<Claim>();
            var localSignInProps = new AuthenticationProperties();
            ProcessLoginCallback(result, additionalLocalClaims, localSignInProps);
            
            // issue authentication cookie for user
            // we must issue the cookie maually, and can't use the SignInManager because
            // it doesn't expose an API to issue additional claims from the login workflow
            var principal = await _signInManager.CreateUserPrincipalAsync(user);
            additionalLocalClaims.AddRange(principal.Claims);
            var name = principal.FindFirst(JwtClaimTypes.Name)?.Value ?? user.Id;
            
            var issuer = new IdentityServerUser(user.Id)
            {
                DisplayName = name,
                IdentityProvider = provider,
                AdditionalClaims = additionalLocalClaims
            };

            await HttpContext.SignInAsync(issuer, localSignInProps);

            // delete temporary cookie used during external authentication
            await HttpContext.SignOutAsync(IdentityServerConstants.ExternalCookieAuthenticationScheme);

            // retrieve return URL
            var returnUrl = result?.Properties?.Items["returnUrl"] ?? "~/";

            // check if external login is in the context of an OIDC request
            var context = await _interaction.GetAuthorizationContextAsync(returnUrl);
            await _events.RaiseAsync(new UserLoginSuccessEvent(provider, providerUserId, user.Id, name, true, context?.Client.ClientId));

            if (context == null) return Redirect(returnUrl);
            
            return context.IsNativeClient() 
                // The client is native, so this change in how to
                // return the response is for better UX for the end user.
                ? this.LoadingPage("Redirect", returnUrl) 
                : Redirect(returnUrl);
        }

        async Task<(ApplicationUser? user, string provider, string providerUserId, IReadOnlyCollection<Claim> claims)> FindUserFromExternalProviderAsync(AuthenticateResult? result)
        {
            var externalUser = result?.Principal;

            // try to determine the unique id of the external user (issued by the provider)
            // the most common claim type for that are the sub claim and the NameIdentifier
            // depending on the external provider, some other claim type might be used
            var userIdClaim = externalUser?.FindFirst(JwtClaimTypes.Subject) 
                              ?? externalUser?.FindFirst(ClaimTypes.NameIdentifier) 
                              ?? throw new Exception("Unknown userid");

            // remove the user id claim so we don't include it as an extra claim if/when we provision the user
            var claims = externalUser.Claims.ToHashSet();
            claims.Remove(userIdClaim);

            var provider = result?.Properties?.Items["scheme"] ?? "Unknown provider";
            var providerUserId = userIdClaim.Value;

            // find external user
            var user = await _userManager.FindByLoginAsync(provider, providerUserId);

            return (user, provider, providerUserId, claims);
        }

        async Task<ApplicationUser> AutoProvisionUserAsync(string provider, string providerUserId, IReadOnlyCollection<Claim> claims)
        {
            // create a list of claims that we want to transfer into our store
            var filtered = new HashSet<Claim>();

            var name = claims.FirstOrDefault(x => x.Type == JwtClaimTypes.Name)?.Value ?? claims.FirstOrDefault(x => x.Type == ClaimTypes.Name)?.Value;
            var givenName = claims.FirstOrDefault(x => x.Type == JwtClaimTypes.GivenName)?.Value ?? claims.FirstOrDefault(x => x.Type == ClaimTypes.GivenName)?.Value;
            var familyName = claims.FirstOrDefault(x => x.Type == JwtClaimTypes.FamilyName)?.Value ?? claims.FirstOrDefault(x => x.Type == ClaimTypes.Surname)?.Value;
            var email = claims.FirstOrDefault(x => x.Type == JwtClaimTypes.Email)?.Value ?? claims.FirstOrDefault(x => x.Type == ClaimTypes.Email)?.Value;
            var phone = claims.FirstOrDefault(x => x.Type == JwtClaimTypes.PhoneNumber)?.Value ?? claims.FirstOrDefault(x => x.Type == ClaimTypes.MobilePhone)?.Value;
            
            if (!string.IsNullOrEmpty(name) || !string.IsNullOrEmpty(givenName) || !string.IsNullOrEmpty(familyName)) 
                filtered.Add(new Claim(JwtClaimTypes.Name, name ?? $"{givenName} {familyName}".Trim()));
            if (!string.IsNullOrEmpty(givenName)) 
                filtered.Add(new Claim(JwtClaimTypes.Name, givenName));
            if (!string.IsNullOrEmpty(familyName)) 
                filtered.Add(new Claim(JwtClaimTypes.Name, familyName));
            if (!string.IsNullOrEmpty(email)) 
                filtered.Add(new Claim(JwtClaimTypes.Email, email));
            if (!string.IsNullOrEmpty(phone)) 
                filtered.Add(new Claim(JwtClaimTypes.Email, phone));

            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                GivenName = givenName,
                FamilyName = familyName,
                Name = name,
                PhoneNumber = phone,
                EmailConfirmed = !string.IsNullOrEmpty(email),
                PhoneNumberConfirmed = !string.IsNullOrEmpty(phone)
            };
            
            var identityResult = await _userManager.CreateAsync(user);
            if (!identityResult.Succeeded) throw new Exception(identityResult.Errors.First().Description);

            if (filtered.Any())
            {
                identityResult = await _userManager.AddClaimsAsync(user, filtered);
                if (!identityResult.Succeeded) throw new Exception(identityResult.Errors.First().Description);
            }

            identityResult = await _userManager.AddLoginAsync(user, new UserLoginInfo(provider, providerUserId, provider));
            if (!identityResult.Succeeded) throw new Exception(identityResult.Errors.First().Description);

            return user;
        }

        // if the external login is OIDC-based, there are certain things we need to preserve to make logout work
        // this will be different for WS-Fed, SAML2p or other protocols
        static void ProcessLoginCallback(AuthenticateResult? externalResult, ICollection<Claim> localClaims, AuthenticationProperties localSignInProps)
        {
            // if the external system sent a session id claim, copy it over
            // so we can use it for single sign-out
            var sid = externalResult?.Principal?.Claims.FirstOrDefault(x => x.Type == JwtClaimTypes.SessionId);
            if (sid != default) localClaims.Add(new Claim(JwtClaimTypes.SessionId, sid.Value));

            // if the external provider issued an id_token, we'll keep it for signout
            var idToken = externalResult?.Properties?.GetTokenValue("id_token");
            if (!string.IsNullOrEmpty(idToken)) localSignInProps.StoreTokens(new[] {new AuthenticationToken {Name = "id_token", Value = idToken}});
        }
    }
}