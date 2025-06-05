using FluentAssertions;
using Frederikskaj2.Reservations.Server.IntegrationTests.Harness;
using LightBDD.Framework;
using LightBDD.XUnit2;
using NodaTime;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace Frederikskaj2.Reservations.Server.IntegrationTests.Tests.Users;

partial class SignOut(SessionFixture session) : FeatureFixture, IClassFixture<SessionFixture>
{
    State<string> accessToken1;
    State<IEnumerable<string>> cookies1;

    string AccessToken1 => accessToken1.GetValue(nameof(AccessToken1));
    IEnumerable<string> Cookies1 => cookies1.GetValue(nameof(Cookies1));

    async Task GivenAUserSignsInOnDevice1()
    {
        session.NowOffset = Period.Zero;
        await session.SignUpAndSignIn();
        accessToken1 = session.AccessToken!;
        cookies1 = new(session.Cookies!);
    }

    async Task GivenAUserSignsInOnDevice2() => await session.SignIn();

    async Task WhenTheUserSignsOutEverywhereElse() => await session.SignOutEverywhereElse();

    async Task ThenTheUserIsNoLongerAbleToAccessTheSystemFromDevice1()
    {
        session.AccessToken = AccessToken1;
        session.Cookies = Cookies1;
        var response = await session.CreateAccessTokenRaw();
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
