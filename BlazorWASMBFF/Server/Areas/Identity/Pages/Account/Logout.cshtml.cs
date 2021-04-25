using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using BlazorWASMBFF.Server.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authentication;
using IdentityServer4;
using IdentityModel;
using IdentityServer4.Extensions;
using IdentityServer4.Services;

namespace BlazorWASMBFF.Server.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class LogoutModel : PageModel
    {
        public class LogoutInputModel
        {
            public string LogoutId { get; set; }
        }
        public class LogoutViewModel : LogoutInputModel
        {
            public bool ShowLogoutPrompt { get; set; }
        }

        public class LoggedOutViewModel
        {
            public string PostLogoutRedirectUri { get; set; }
            public string ClientName { get; set; }
            public string SignOutIframeUrl { get; set; }

            public bool AutomaticRedirectAfterSignOut { get; set; }

            public string LogoutId { get; set; }
            public bool TriggerExternalSignout => ExternalAuthenticationScheme != null;
            public string ExternalAuthenticationScheme { get; set; }
        }

        private readonly ILogger<LogoutModel> _logger;
        private readonly IIdentityServerInteractionService _interaction;
        public LogoutModel(ILogger<LogoutModel> logger, IIdentityServerInteractionService interaction)
        {
            _interaction = interaction;
            _logger = logger;
        }
        public LoggedOutViewModel LoggedOut;
        public async Task OnGetAsync(string logoutId)
        {
            // build a model so the logout page knows what to display
            var vm = await BuildLogoutViewModelAsync(logoutId);

            if (vm.ShowLogoutPrompt == false)
            {
                // if the request for logout was properly authenticated from IdentityServer, then
                // we don't need to show the prompt and can just log the user out directly.
                await OnPost(vm);
            }
        }

        private async Task<LoggedOutViewModel> BuildLoggedOutViewModelAsync(string logoutId)
        {
            // get context information (client name, post logout redirect URI and iframe for federated signout)
            var logout = await _interaction.GetLogoutContextAsync(logoutId);

            var vm = new LoggedOutViewModel
            {
                AutomaticRedirectAfterSignOut = true,
                PostLogoutRedirectUri = "/",
                ClientName = string.IsNullOrEmpty(logout?.ClientName) ? logout?.ClientId : logout?.ClientName,
                SignOutIframeUrl = logout?.SignOutIFrameUrl,
                LogoutId = logoutId
            };

            if (User?.Identity.IsAuthenticated == true)
            {
                var idp = User.FindFirst(JwtClaimTypes.IdentityProvider)?.Value;
                if (idp != null && idp != IdentityServer4.IdentityServerConstants.LocalIdentityProvider)
                {
                    var providerSupportsSignout = await HttpContext.GetSchemeSupportsSignOutAsync(idp);
                    if (providerSupportsSignout)
                    {
                        if (vm.LogoutId == null)
                        {
                            // if there's no current logout context, we need to create one
                            // this captures necessary info from the current logged in user
                            // before we signout and redirect away to the external IdP for signout
                            vm.LogoutId = await _interaction.CreateLogoutContextAsync();
                        }

                        vm.ExternalAuthenticationScheme = idp;
                    }
                }
            }

            return vm;
        }

        private async Task<LogoutViewModel> BuildLogoutViewModelAsync(string logoutId)
        {
            var vm = new LogoutViewModel { LogoutId = logoutId, ShowLogoutPrompt = false };

            if (User?.Identity.IsAuthenticated != true)
            {
                // if the user is not authenticated, then just show logged out page
                vm.ShowLogoutPrompt = false;
                return vm;
            }

            var context = await _interaction.GetLogoutContextAsync(logoutId);
            if (context?.ShowSignoutPrompt == false)
            {
                // it's safe to automatically sign-out
                vm.ShowLogoutPrompt = false;
                return vm;
            }

            // show the logout prompt. this prevents attacks where the user
            // is automatically signed out by another malicious web page.
            return vm;
        }
        public async Task OnPost(LogoutInputModel model)
        { // build a model so the logged out page knows what to display
            LoggedOut = await BuildLoggedOutViewModelAsync(model.LogoutId);

            //  if (User?.Identity.IsAuthenticated == true)
            {
                await HttpContext.SignOutAsync(IdentityServerConstants.DefaultCookieAuthenticationScheme);

                // raise the logout event

            }

            _logger.LogInformation("User logged out.");

        }
    }
}
