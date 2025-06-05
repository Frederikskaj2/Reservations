using FluentAssertions;
using Frederikskaj2.Reservations.Server.IntegrationTests.Harness;
using LightBDD.Core.Extensibility.Execution;
using LightBDD.Framework;
using LightBDD.XUnit2;
using NodaTime;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Frederikskaj2.Reservations.Server.IntegrationTests.Tests.Users;

sealed partial class SignIn(SessionFixture session) : FeatureFixture, IScenarioSetUp, IClassFixture<SessionFixture>
{
    // These can be configured using appsettings.*.json but these are the default
    // values when they're not configured.
    const int configuredMaximumAllowedFailedSignInAttempts = 10;
    static readonly Period configuredLockoutPeriod = Period.FromMinutes(5);

    State<HttpResponseMessage> response;

    HttpResponseMessage Response => response.GetValue(nameof(Response));

    Task IScenarioSetUp.OnScenarioSetUp()
    {
        session.NowOffset = Period.Zero;
        return Task.CompletedTask;
    }

    async Task GivenAUserSignsUp() => await session.SignUp();

    Task GivenTheUserSignsInWithAnIncorrectPassword() => SignInWithBadPassword(triggerLockout: false);

    Task GivenTheUserSignsInWithAnIncorrectPasswordTooManyTimes() => SignInWithBadPassword(triggerLockout: true);

    Task GivenTheUserWaitsForTheLockoutPeriodToExpire()
    {
        session.NowOffset = configuredLockoutPeriod;
        return Task.CompletedTask;
    }

    async Task GivenTheUserSignsInWithTheCorrectPassword() => response = await session.SignInRaw(session.User!.Email, session.User.Password);

    async Task WhenTheUserSignsInWithTheCorrectPassword() => response = await session.SignInRaw(session.User!.Email, session.User.Password);

    Task ThenTheUserIsNotSignedIn()
    {
        Response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        return Task.CompletedTask;
    }

    Task ThenTheUserIsSignedIn()
    {
        Response.StatusCode.Should().Be(HttpStatusCode.OK);
        return Task.CompletedTask;
    }

    async Task SignInWithBadPassword(bool triggerLockout)
    {
        var count = triggerLockout ? configuredMaximumAllowedFailedSignInAttempts : configuredMaximumAllowedFailedSignInAttempts - 1;
        const string badPassword = ".";
        for (var i = 0; i < count; i += 1)
        {
            var forbiddenResponse = await session.SignInRaw(session.User!.Email, badPassword);
            forbiddenResponse.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }
    }
}
