using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer.Areas.Grants.Models;
using IdentityServer.ViewModels;
using IdentityServer4.Events;
using IdentityServer4.Extensions;
using IdentityServer4.Models;
using IdentityServer4.Services;
using IdentityServer4.Stores;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IdentityServer.Areas.Grants.Controllers
{
    [Area("Grants"), Authorize(Policy = "All")]
    public class GrantsController : Controller
    {
        readonly IIdentityServerInteractionService _interaction;
        readonly IClientStore _clients;
        readonly IResourceStore _resources;
        readonly IEventService _events;

        public GrantsController(IIdentityServerInteractionService interaction, IClientStore clients, IResourceStore resources, IEventService events)
        {
            _interaction = interaction;
            _clients = clients;
            _resources = resources;
            _events = events;
        }

        [HttpGet]
        public async Task<IActionResult> Index() => View("Index", await BuildViewModelAsync());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Revoke(string? clientId = default)
        {
            if (string.IsNullOrEmpty(clientId)) return View("Error", new ErrorViewModel("Invalid ClientId"));
            
            await _interaction.RevokeUserConsentAsync(clientId);
            await _events.RaiseAsync(new GrantsRevokedEvent(User.GetSubjectId(), clientId));

            return RedirectToAction("Index");
        }

        async Task<GrantsViewModel> BuildViewModelAsync()
        {
            var grants = await _interaction.GetAllUserGrantsAsync();

            var list = new List<GrantViewModel>();
            foreach(var grant in grants)
            {
                var client = await _clients.FindClientByIdAsync(grant.ClientId);
                if (client == null) continue;
                
                var resources = await _resources.FindResourcesByScopeAsync(grant.Scopes);

                var item = new GrantViewModel(client.ClientId, client.ClientName, client.LogoUri, client.ClientUri, grant.Description,
                    grant.CreationTime, grant.Expiration,
                    resources.IdentityResources.Select(x => x.DisplayName ?? x.Name),
                    resources.ApiScopes.Select(x => x.DisplayName ?? x.Name));

                list.Add(item);
            }

            return new GrantsViewModel(list);
        }
    }
}