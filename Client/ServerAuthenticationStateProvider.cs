using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using Frederikskaj2.Reservations.Shared;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace Frederikskaj2.Reservations.Client
{
    public class ServerAuthenticationStateProvider : AuthenticationStateProvider
    {
        private readonly HttpClient httpClient;

        public ServerAuthenticationStateProvider(HttpClient httpClient) => this.httpClient = httpClient;

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var userInfo = await httpClient.GetJsonAsync<UserInfo>("user");

            var identity = userInfo.IsAuthenticated
                ? new ClaimsIdentity(new[] {new Claim(ClaimTypes.Name, userInfo.Name)}, "serverauth")
                : new ClaimsIdentity();

            return new AuthenticationState(new ClaimsPrincipal(identity));
        }
    }
}