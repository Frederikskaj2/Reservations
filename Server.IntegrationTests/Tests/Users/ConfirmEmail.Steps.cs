using FluentAssertions;
using Frederikskaj2.Reservations.Server.IntegrationTests.Harness;
using LightBDD.XUnit2;
using System.Threading.Tasks;
using Xunit;

namespace Frederikskaj2.Reservations.Server.IntegrationTests.Tests.Users;

partial class ConfirmEmail(SessionFixture session) : FeatureFixture, IClassFixture<SessionFixture>
{
    async Task GivenAUserIsSignedUp() => await session.SignUp();

    async Task GivenTheUserConfirmsTheirEmail() => await session.ConfirmEmail();

    async Task GivenTheUserDidNotReceiveTheConfirmEmailEmail() => await session.DequeueConfirmEmailEmail();

    async Task GivenTheUserSignsUpAgainUsingTheSameEmail() => await session.SignUpAgain();

    async Task WhenTheUserDoesNotConfirmTheirEmail() => await session.DequeueConfirmEmailEmail();

    async Task WhenTheUserConfirmsTheirEmail() => await session.ConfirmEmail();

    async Task WhenTheUserSignsUpAgainUsingTheSameEmail() => await session.SignUpAgain();

    async Task WhenTheUserHasTheConfirmEmailEmailSentAgain()
    {
        await session.SignIn();
        await session.ResendConfirmEmailEmail();
    }

    async Task ThenTheUsersEmailIsNotConfirmed()
    {
        // Set the user ID.
        await session.SignIn();
        var getUserResponse = await session.GetUser(session.UserId());
        getUserResponse.User.IsEmailConfirmed.Should().BeFalse();
    }

    async Task ThenTheUsersEmailIsConfirmed()
    {
        // Set the user ID.
        await session.SignIn();
        var getUserResponse = await session.GetUser(session.UserId());
        getUserResponse.User.IsEmailConfirmed.Should().BeTrue();
    }

    async Task ThenTheUserReceivesTheConfirmEmailEmail() => await session.DequeueConfirmEmailEmail();
}
