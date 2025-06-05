using LightBDD.Framework;
using LightBDD.Framework.Scenarios;
using LightBDD.XUnit2;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Server.IntegrationTests.Tests.Users;

[FeatureDescription(
    """
    As a user
    I want to confirm my email
    So that I can prove that it's my email
    """)]
public partial class ConfirmEmail
{
    [Scenario]
    public Task UserSignsUpButDoesNotConfirmTheirEmail() =>
        Runner.RunScenarioAsync(
            GivenAUserIsSignedUp,
            WhenTheUserDoesNotConfirmTheirEmail,
            ThenTheUsersEmailIsNotConfirmed);

    [Scenario]
    public Task UserSignsUpAndConfirmsTheirEmail() =>
        Runner.RunScenarioAsync(
            GivenAUserIsSignedUp,
            WhenTheUserConfirmsTheirEmail,
            ThenTheUsersEmailIsConfirmed);

    [Scenario]
    public Task UserSignsUpAndConfirmsTheirEmailAndSignsUpAgain() =>
        Runner.RunScenarioAsync(
            GivenAUserIsSignedUp,
            GivenTheUserConfirmsTheirEmail,
            WhenTheUserSignsUpAgainUsingTheSameEmail,
            ThenTheUsersEmailIsConfirmed);

    [Scenario]
    public Task UserSignsUpTwiceAndConfirmsTheirEmail() =>
        Runner.RunScenarioAsync(
            GivenAUserIsSignedUp,
            GivenTheUserSignsUpAgainUsingTheSameEmail,
            WhenTheUserConfirmsTheirEmail,
            ThenTheUsersEmailIsConfirmed);

    [Scenario]
    public Task UserSignsUpAndHasTheConfirmEmailResent() =>
        Runner.RunScenarioAsync(
            GivenAUserIsSignedUp,
            GivenTheUserDidNotReceiveTheConfirmEmailEmail,
            WhenTheUserHasTheConfirmEmailEmailSentAgain,
            ThenTheUserReceivesTheConfirmEmailEmail);
}
