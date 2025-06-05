using LightBDD.Framework;
using LightBDD.Framework.Scenarios;
using LightBDD.XUnit2;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Server.IntegrationTests.Tests.Users;

[FeatureDescription(
    """
    As a user
    I want to be able to control my password
    So that I can protect access to my account
    """)]
public partial class Password
{
    [Scenario]
    public Task UpdatePassword() =>
        Runner.RunScenarioAsync(
            GivenAUserIsSignedIn,
            WhenTheyUpdateTheirPassword,
            ThenTheyCanSignInWithTheNewPassword);

    [Scenario]
    public Task UpdatePasswordWithInvalidCookie() =>
        Runner.RunScenarioAsync(
            GivenAnotherUserIsAuthenticatedUsingCookies,
            GivenAUserIsSignedIn,
            WhenTheUserTriesToUpdateTheirPasswordWithTheOtherUsersCookies,
            ThenTheRequestIsForbidden);

    [Scenario]
    public Task UpdatePasswordWithNewPasswordEmail() =>
        Runner.RunScenarioAsync(
            GivenAUserThatIsNotSignedIn,
            WhenTheUserRequestANewPasswordEmail,
            ThenTheUserReceivesAnEmailWithALinkThatIsUsedToUpdateTheirPassword);

    [Scenario]
    public Task UpdatePasswordBySigningUpTwice() =>
        Runner.RunScenarioAsync(
            GivenAUserThatIsNotSignedIn,
            WhenTheUserSignsUpAgainWithTheSameEmail,
            ThenTheUserReceivesAnEmailWithALinkThatIsUsedToUpdateTheirPassword);

    [Scenario]
    public Task ProbeEmailsByUsingTheNewPasswordEmail() =>
        Runner.RunScenarioAsync(
            GivenAnEmailNotUsedByAUser,
            WhenANewPasswordIsRequestedForTheEmail,
            ThenTheRequestIsSuccessfulDespiteNoEmailIsSent);
}
