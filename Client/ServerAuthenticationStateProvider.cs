using System.Collections.Generic;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using Frederikskaj2.Reservations.Shared;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace Frederikskaj2.Reservations.Client
{
    public class ServerAuthenticationStateProvider : AuthenticationStateProvider, IAuthenticationStateProvider
    {
        private readonly HttpClient httpClient;

        public ServerAuthenticationStateProvider(HttpClient httpClient) => this.httpClient = httpClient;

        public override async Task<AuthenticationState> GetAuthenticationStateAsync() => await GetAuthenticationState(httpClient.GetJsonAsync<UserInfo>("user"));

        public void UpdateUser(UserInfo user) => NotifyAuthenticationStateChanged(GetAuthenticationState(Task.FromResult(user)));

        private static async Task<AuthenticationState> GetAuthenticationState(Task<UserInfo> userTask)
        {
            var user = await userTask;

            ClaimsIdentity GetIdentity()
            {
                if (!user.IsAuthenticated)
                    return new ClaimsIdentity();
                var claims = new List<Claim> {new Claim(ClaimTypes.Name, user.Name)};
                if (user.IsAdministrator)
                    claims.Add(new Claim(ClaimTypes.Role, "Administrator"));
                return new ClaimsIdentity(claims, "serverauth");
            }

            var identity = GetIdentity();

            return new AuthenticationState(new ClaimsPrincipal(identity));
        }
    }
}