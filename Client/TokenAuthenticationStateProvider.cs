using Frederikskaj2.Reservations.Shared.Web;
using Microsoft.AspNetCore.Components.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Client;

public sealed class TokenAuthenticationStateProvider : AuthenticationStateProvider, IDisposable
{
    static readonly AuthenticationState notAuthenticated = new(new ClaimsPrincipal(new ClaimsIdentity()));

    readonly AuthenticationService authenticationService;
    readonly IDisposable signInSubscription;
    readonly IDisposable signOutSubscription;

    public TokenAuthenticationStateProvider(AuthenticationService authenticationService, EventAggregator eventAggregator)
    {
        this.authenticationService = authenticationService;

        signInSubscription = eventAggregator.Subscribe<SignInMessage>(_ => NotifyAuthenticationStateChanged(GetAuthenticationStateAsync()));
        signOutSubscription = eventAggregator.Subscribe<SignOutMessage>(_ => NotifyAuthenticationStateChanged(Task.FromResult(notAuthenticated)));
    }

    public void Dispose()
    {
        signInSubscription.Dispose();
        signOutSubscription.Dispose();
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var accessToken = await authenticationService.GetAccessTokenAsync();
        return string.IsNullOrEmpty(accessToken) ? notAuthenticated : GetAuthenticatedUser(accessToken);
    }

    static AuthenticationState GetAuthenticatedUser(string accessToken)
    {
        const string authenticationType = "token";
        var claims = GetClaims(accessToken).ToList();
        return claims.Count != 0 ? new(new(new ClaimsIdentity(claims, authenticationType))) : notAuthenticated;
    }

    static IEnumerable<Claim> GetClaims(string token)
    {
        var claims = JwtTokenParser.Parse(token);
        if (claims is null)
            yield break;
        foreach (var (claimName, element) in claims)
            if (element.ValueKind == JsonValueKind.Array)
                foreach (var value in element.EnumerateArray().Select(subElement => subElement.ToString()).Where(value => value is { Length: > 0 }))
                    yield return new Claim(claimName, value);
            else
            {
                var value = element.ToString();
                if (value is { Length: > 0 })
                    yield return new Claim(claimName, value);
            }
    }
}
