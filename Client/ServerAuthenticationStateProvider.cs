using System;
using System.Collections.Generic;
using System.Globalization;
using System.Security.Claims;
using System.Threading.Tasks;
using Frederikskaj2.Reservations.Shared;
using Microsoft.AspNetCore.Components.Authorization;

namespace Frederikskaj2.Reservations.Client
{
    public class ServerAuthenticationStateProvider : AuthenticationStateProvider, IAuthenticationStateProvider
    {
        private readonly ApiClient apiClient;

        public ServerAuthenticationStateProvider(ApiClient apiClient)
            => this.apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));

        public void UpdateUser(AuthenticatedUser user)
            => NotifyAuthenticationStateChanged(
                GetAuthenticationState(Task.FromResult((Maybe<AuthenticatedUser>) user)));

        public override Task<AuthenticationState> GetAuthenticationStateAsync()
            => GetAuthenticationState(apiClient.GetJsonAsync<AuthenticatedUser>("user/authenticated"));

        private static async Task<AuthenticationState> GetAuthenticationState(Task<Maybe<AuthenticatedUser>> userTask)
        {
            var maybe = await userTask;
            maybe.TryGetValue(out var user);
            return new AuthenticationState(new ClaimsPrincipal(GetIdentity()));

            ClaimsIdentity GetIdentity()
            {
                const string authenticationType = "serverauth";
                return user.IsAuthenticated
                    ? new ClaimsIdentity(GetClaims(), authenticationType)
                    : new ClaimsIdentity();

                IEnumerable<Claim> GetClaims()
                {
                    yield return new Claim(ClaimTypes.Name, user.Name);
                    if (user.Id.HasValue)
                        yield return new Claim(
                            ClaimTypes.NameIdentifier, user.Id.Value.ToString(CultureInfo.InvariantCulture));
                    if (user.Roles != null)
                        foreach (var role in user.Roles)
                            yield return new Claim(ClaimTypes.Role, role);
                }
            }
        }
    }
}