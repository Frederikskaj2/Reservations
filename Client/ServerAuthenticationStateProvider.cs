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
                GetAuthenticationState(Task.FromResult<(AuthenticatedUser? Response, ProblemDetails? Problem)>((user, null))));

        public override Task<AuthenticationState> GetAuthenticationStateAsync()
            => GetAuthenticationState(apiClient.Get<AuthenticatedUser>("user/authenticated"));

        private static async Task<AuthenticationState> GetAuthenticationState(Task<(AuthenticatedUser? Response, ProblemDetails? Problem)> userTask)
        {
            var (response, _) = await userTask;
            return new AuthenticationState(new ClaimsPrincipal(GetIdentity()));

            ClaimsIdentity GetIdentity()
            {
                const string authenticationType = "serverauth";
                return (response?.IsAuthenticated).GetValueOrDefault()
                    ? new ClaimsIdentity(GetClaims(), authenticationType)
                    : new ClaimsIdentity();

                IEnumerable<Claim> GetClaims()
                {
                    yield return new Claim(ClaimTypes.Name, response?.Name ?? "?");
                    if ((response?.Id).HasValue)
                        yield return new Claim(
                            ClaimTypes.NameIdentifier, response!.Id!.Value.ToString(CultureInfo.InvariantCulture));
                    if (response?.Roles != null)
                        foreach (var role in response.Roles)
                            yield return new Claim(ClaimTypes.Role, role);
                }
            }
        }
    }
}