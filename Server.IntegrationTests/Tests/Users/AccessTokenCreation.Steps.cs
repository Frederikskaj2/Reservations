using FluentAssertions;
using Frederikskaj2.Reservations.Server.IntegrationTests.Harness;
using Frederikskaj2.Reservations.Users;
using LanguageExt;
using LightBDD.Core.Extensibility.Execution;
using LightBDD.Framework;
using LightBDD.XUnit2;
using Microsoft.Extensions.DependencyInjection;
using NodaTime;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using Xunit.Sdk;
using Cookie = Frederikskaj2.Reservations.Users.Cookie;

namespace Frederikskaj2.Reservations.Server.IntegrationTests.Tests.Users;

sealed partial class AccessTokenCreation(SessionFixture session) : FeatureFixture, IScenarioSetUp, IClassFixture<SessionFixture>
{
    State<string> accessToken;
    State<IEnumerable<string>> cookies;
    State<Cookie> cookie;
    State<TokensResponse> tokensResponse;
    State<HttpResponseMessage> apiResponse;

    string AccessToken => accessToken.GetValue(nameof(AccessToken));
    IEnumerable<string> Cookies => cookies.GetValue(nameof(Cookies));
    Cookie Cookie => cookie.GetValue(nameof(Cookie));
    TokensResponse TokensResponse => tokensResponse.GetValue(nameof(TokensResponse));
    HttpResponseMessage ApiResponse => apiResponse.GetValue(nameof(ApiResponse));

    Task IScenarioSetUp.OnScenarioSetUp()
    {
        session.NowOffset = Period.Zero;
        return Task.CompletedTask;
    }

    async Task GivenAClientIsSignedIn()
    {
        session.NowOffset = Period.Zero;
        await session.SignUpAndSignIn();
        accessToken = session.AccessToken!;
        cookies = new(session.Cookies!);
    }

    async Task GivenTheClientSignsInAgain() => await session.SignIn();

    Task GivenTheRefreshTokenHasExpired()
    {
        session.NowOffset = Period.FromDays(1);
        return Task.CompletedTask;
    }

    async Task GivenTheRefreshTokenIsInvalid()
    {
        using var serviceScope = session.CreateServiceScope();
        var refreshTokenCookieService = serviceScope.ServiceProvider.GetRequiredService<IRefreshTokenCookieService>();
        var parsedRefreshToken = await refreshTokenCookieService.ParseCookie<Unit>(string.Join(';', session.Cookies!)).Match(
            token => token,
            _ => throw new XunitException("Cannot parse cookie."));
        var invalidTokenId = parsedRefreshToken.TokenId.NextId;
        cookie = refreshTokenCookieService.CreateCookie(session.UserId(), invalidTokenId, isPersistent: false);
    }

    async Task WhenAnAccessTokenIsCreated() => tokensResponse = await session.CreateAccessToken();

    async Task WhenAnAccessTokenIsCreated1() => apiResponse = await session.CreateAccessTokenRaw();

    async Task WhenAnAccessTokenIsCreatedUsingTheInvalidRefreshToken()
    {
        var cookieHeaderValue = $"{Cookie.Name}={Cookie.Value}";
        apiResponse = await session.CreateAccessTokenRaw([cookieHeaderValue]);
    }

    async Task WhenAnAccessTokenIsCreatedUsingTheCookieFromTheFirstSignIn()
    {
        session.AccessToken = AccessToken;
        session.Cookies = Cookies;
        tokensResponse = await session.CreateAccessToken();
    }

    Task ThenTheAccessTokenShouldHaveAValue()
    {
        TokensResponse.AccessToken.Should().NotBeNullOrEmpty();
        return Task.CompletedTask;
    }

    Task ThenAccessIsForbidden()
    {
        ApiResponse.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        return Task.CompletedTask;
    }
}
