﻿using System.Collections.Generic;
using System.Globalization;
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

        public void UpdateUser(AuthenticatedUser user)
            => NotifyAuthenticationStateChanged(GetAuthenticationState(Task.FromResult(user)));

        public override Task<AuthenticationState> GetAuthenticationStateAsync()
            => GetAuthenticationState(httpClient.GetJsonAsync<AuthenticatedUser>("user/authenticated"));

        private static async Task<AuthenticationState> GetAuthenticationState(Task<AuthenticatedUser> userTask)
        {
            var user = await userTask;
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
                    if (user.IsAdministrator)
                        yield return new Claim(ClaimTypes.Role, Roles.Administrator);
                }
            }
        }
    }
}