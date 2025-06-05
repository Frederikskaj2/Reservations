using FluentAssertions;
using Frederikskaj2.Reservations.Server.IntegrationTests.Harness;
using Frederikskaj2.Reservations.Users;
using LightBDD.Framework;
using LightBDD.XUnit2;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Frederikskaj2.Reservations.Server.IntegrationTests.Tests.Users;

partial class Password(SessionFixture session) : FeatureFixture, IClassFixture<SessionFixture>
{
    State<IEnumerable<string>> anotherUsersCookies;
    State<string> email;
    State<HttpResponseMessage> response;

    IEnumerable<string> AnotherUsersCookies => anotherUsersCookies.GetValue(nameof(AnotherUsersCookies));
    string Email => email.GetValue(nameof(Email));
    HttpResponseMessage Response => response.GetValue(nameof(Response));

    async Task GivenAUserIsSignedIn() => await session.SignUpAndSignIn();

    async Task GivenAnotherUserIsAuthenticatedUsingCookies()
    {
        await session.SignUpAndSignIn();
        anotherUsersCookies = new(session.Cookies!);
    }

    async Task GivenAUserThatIsNotSignedIn() => await session.SignUp();

    Task GivenAnEmailNotUsedByAUser()
    {
        email = Generate.Email();
        return Task.CompletedTask;
    }

    async Task WhenTheyUpdateTheirPassword() => await session.UpdatePassword();

    async Task WhenTheUserTriesToUpdateTheirPasswordWithTheOtherUsersCookies()
    {
        session.Cookies = AnotherUsersCookies;
        response = await session.UpdatePasswordRaw();
    }

    async Task WhenTheUserRequestANewPasswordEmail() => await session.SendNewPasswordEmail();

    async Task WhenTheUserSignsUpAgainWithTheSameEmail()
    {
        var request = new SignUpRequest(email: session.User!.Email, fullName: session.User.FullName, phone: session.User.Phone,
            apartmentId: session.User.ApartmentId, password: Generate.Password());
        await session.SignUpRaw(request);
    }

    async Task WhenANewPasswordIsRequestedForTheEmail() => response = await session.SendNewPasswordEmailRaw(Email);

    async Task ThenTheyCanSignInWithTheNewPassword() => await session.SignIn();

    Task ThenTheRequestIsForbidden()
    {
        Response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        return Task.CompletedTask;
    }

    async Task ThenTheUserReceivesAnEmailWithALinkThatIsUsedToUpdateTheirPassword() => await session.NewPasswordFromNewPasswordEmail();

    Task ThenTheRequestIsSuccessfulDespiteNoEmailIsSent()
    {
        Response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        return Task.CompletedTask;
    }
}
