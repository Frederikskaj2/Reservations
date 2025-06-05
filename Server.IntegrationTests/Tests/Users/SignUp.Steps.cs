using Frederikskaj2.Reservations.Server.IntegrationTests.Harness;
using LightBDD.Framework;
using LightBDD.XUnit2;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Frederikskaj2.Reservations.Server.IntegrationTests.Tests.Users;

partial class SignUp(SessionFixture session) : FeatureFixture, IClassFixture<SessionFixture>
{
    State<HttpResponseMessage> response;

    HttpResponseMessage Response => response.GetValue(nameof(Response));

    async Task GivenAUserSignsUp() => await session.SignUp();

    async Task GivenTheUserSignsUpAgainWithTheSameInformation() => await session.SignUpAgain();

    async Task GivenTheUserCreatesANewPasswordUsingTheEmailTheyReceived() => await session.NewPasswordFromNewPasswordEmail();

    async Task WhenTheUserSignsInWithTheirPassword() => response = await session.SignInRaw(session.User!.Email, session.User.Password);

    Task ThenTheyAreSignedIn()
    {
        Response.EnsureSuccessStatusCode();
        return Task.CompletedTask;
    }
}
