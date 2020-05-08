﻿using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using System.Threading.Tasks;
using SGNWebAppCli.Services;
using System.Net.Http;

namespace SGNWebAppCli.Data
{
    public class CustomAuthenticationStateProvider : AuthenticationStateProvider
    {
        // Zmiana z sessionstorage na local
        // private ISessionStorageService _sessionStorageService;
        private ILocalStorageService _localStorageService;
        public IUserService _userService;
        private readonly HttpClient _httpClient;
        public CustomAuthenticationStateProvider(ILocalStorageService localStorageService, IUserService userService,HttpClient httpClient)
        {
            _localStorageService = localStorageService;
            _userService = userService;
            _httpClient = httpClient;
        }


        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {

            // var emailAddress = await _localStorageService.GetItemAsync<string>("emailAddress");
            var accessToken = await _localStorageService.GetItemAsync<string>("accessToken");
            ClaimsIdentity identity;
            // if (emailAddress != null)
            if (string.IsNullOrEmpty(accessToken))
            {
                User user = await _userService.GetUserByAccessTokenAsync(accessToken);
              
                identity = GetClaimsIdentity(user);
                // identity = new ClaimsIdentity(new[]
                //{
                //     new Claim(ClaimTypes.Name,emailAddress),
                // }, "apiauth_type");
            }
            else
            {
                identity = new ClaimsIdentity();
            }


            // var user = new ClaimsPrincipal(identity);
            var claimsPrincipal = new ClaimsPrincipal(identity);
            return await Task.FromResult(new AuthenticationState(claimsPrincipal));
        }

        public async void MarkUserAsAuthenticated(User user)
        {
            await _localStorageService.SetItemAsync("accessToken", user.AccessToken);
            await _localStorageService.SetItemAsync("refreshToken", user.RefreshToken);
            var identity = GetClaimsIdentity(user);
            // var identity = new ClaimsIdentity(new[]
            //{
            //     new Claim(ClaimTypes.Name,emailAddress),
            // }, "apiauth_type");

            var claimsPrincipal = new ClaimsPrincipal(identity);
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(claimsPrincipal)));
        }

        public void MarkUserAsLoggedOut()
        {
            _localStorageService.RemoveItemAsync("accessToken");
            _localStorageService.RemoveItemAsync("refreshToken");
            var identity = new ClaimsIdentity();
            var user = new ClaimsPrincipal(identity);

            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
        }

        private ClaimsIdentity GetClaimsIdentity(User user)
        {
            var claimsIdentity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, user.UserEmailAddress),
                new Claim(ClaimTypes.Role, user.Role.RoleDescription),
                new Claim("IsActiveUser",CheckActive(user))
            }, "apiauth_type");

            return claimsIdentity;
        }

        private string CheckActive(User user)
        {
            return user.IsActive == true ? "true" : "false";
        }
    }
}
