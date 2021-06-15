using System.Linq;
using System.Threading.Tasks;
using IdentityServer.Extensions;
using IdentityServer.ViewModels.Consent;
using IdentityServer4.Events;
using IdentityServer4.Extensions;
using IdentityServer4.Models;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IdentityServer.Controllers.Consent
{
    [Authorize]
    public class ConsentController : Controller
    {
        const bool EnableOfflineAccess = true;
        
        readonly IIdentityServerInteractionService _interaction;
        readonly IEventService _events;

        public ConsentController(IIdentityServerInteractionService interaction, IEventService events)
        {
            _interaction = interaction;
            _events = events;
        }
        
        [HttpGet]
        public async Task<IActionResult> Index(string? returnUrl = default)
        {
            ViewData["ReturnUrl"] = returnUrl;
            var request = await _interaction.GetAuthorizationContextAsync(returnUrl);
            
            return request != default 
                ? View("Index", new ConsentViewModel(request))
                : View("Error");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AllowConsent(ConsentInputModel model, string? returnUrl = default)
        {
            if(string.IsNullOrEmpty(returnUrl))return View("Error");
            
            var request = await _interaction.GetAuthorizationContextAsync(returnUrl);
            if (request == null) return View("Error");
            
            if (!model.ScopesConsented.Any())
            {
                ModelState.AddModelError(string.Empty, "You must pick at least one permission");
                return View("Index", new ConsentViewModel(request, model));
            }
            
            var scopes = model.ScopesConsented;
            if (!EnableOfflineAccess) scopes = scopes.Where(x => x != IdentityServer4.IdentityServerConstants.StandardScopes.OfflineAccess).ToList();

            var grantedConsent = new ConsentResponse
            {
                RememberConsent = model.RememberConsent,
                ScopesValuesConsented = scopes.ToArray(),
                Description = model.Description
            };

            await _events.RaiseAsync(new ConsentGrantedEvent(User.GetSubjectId(), request.Client.ClientId, request.ValidatedResources.RawScopeValues, grantedConsent.ScopesValuesConsented, grantedConsent.RememberConsent));
            
            await _interaction.GrantConsentAsync(request, grantedConsent);
            
            return request.IsNativeClient() 
                ? this.LoadingPage("Redirect", returnUrl) 
                : Redirect(returnUrl);
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DenyConsent(string? returnUrl = default)
        {
            if(string.IsNullOrEmpty(returnUrl))return View("Error");
            
            var request = await _interaction.GetAuthorizationContextAsync(returnUrl);
            if (request == null) return View("Error");
    
            var grantedConsent = new ConsentResponse { Error = AuthorizationError.AccessDenied };

            await _events.RaiseAsync(new ConsentDeniedEvent(User.GetSubjectId(), request.Client.ClientId, request.ValidatedResources.RawScopeValues));

            await _interaction.GrantConsentAsync(request, grantedConsent);
            
            return request.IsNativeClient() 
                ? this.LoadingPage("Redirect", returnUrl) 
                : Redirect(returnUrl);
        }
    }
}