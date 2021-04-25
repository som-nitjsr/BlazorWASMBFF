using BlazorWASMBFF.Shared;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BlazorWASMBFF.Client.Services
{
    public class CustomAuthenticationStateProvider : AuthenticationStateProvider
    {
        
        private UserProfile _user = null;

        private const string LogInPath = "Account/Login";
        private const string LogOutPath = "Account/Logout";

        private readonly NavigationManager _navigation;
        private readonly HttpClient _client;
        private readonly ILogger<CustomAuthenticationStateProvider> _logger;

 

        public CustomAuthenticationStateProvider(NavigationManager navigation, HttpClient client, ILogger<CustomAuthenticationStateProvider> logger)
        {
            _navigation = navigation;
            _client = client;
            _logger = logger;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync() => new AuthenticationState(await LoggedInUser());

        public void SignIn(string customReturnUrl = null)
        {
            var returnUrl = customReturnUrl != null ? _navigation.ToAbsoluteUri(customReturnUrl).ToString() : null;
            var encodedReturnUrl = Uri.EscapeDataString(returnUrl ?? _navigation.Uri);
            var logInUrl = _navigation.ToAbsoluteUri($"{LogInPath}?returnUrl={encodedReturnUrl}");
            _navigation.NavigateTo(logInUrl.ToString(), true);
        }

        public void SignOut()
        {
            _navigation.NavigateTo(_navigation.ToAbsoluteUri(LogOutPath).ToString(), true);
        }

   
      
        private async Task<ClaimsPrincipal> LoggedInUser()
        {
           

            try
            {
                _user = await _client.GetFromJsonAsync<UserProfile>("myapi/me");
              
            }
            catch (Exception exc)
            {
                
            }

            if (_user?.IsAuthenticated==false)
            {
                return new ClaimsPrincipal(new ClaimsIdentity());
            }

            var identity = new ClaimsIdentity(
                nameof(CustomAuthenticationStateProvider),
               "name", "role");
            identity.AddClaim(new Claim("name", _user.UserName));
            identity.AddClaim(new Claim("role", _user.Role));



            return new ClaimsPrincipal(identity);
        }
    }
}
