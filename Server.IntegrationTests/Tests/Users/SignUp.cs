using LightBDD.Framework;
using LightBDD.Framework.Scenarios;
using LightBDD.XUnit2;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Server.IntegrationTests.Tests.Users;

[FeatureDescription(
    """
    As a user
    I want to be able to sign up twice with the same email address
    So that when I mistakenly forgot I already signed up I'm still able to access the system without problems
    """)]
public partial class SignUp
{
    [Scenario]
    public Task SignUpTwiceAndSignInUsingThePasswordCreatedUsingTheEmailShouldSignIn() =>
        Runner.RunScenarioAsync(
            GivenAUserSignsUp,
            GivenTheUserSignsUpAgainWithTheSameInformation,
            GivenTheUserCreatesANewPasswordUsingTheEmailTheyReceived,
            WhenTheUserSignsInWithTheirPassword,
            ThenTheyAreSignedIn);

    [Scenario]
    public Task SignUpTwiceAndSignInUsingPasswordShouldSignIn() =>
        Runner.RunScenarioAsync(
            GivenAUserSignsUp,
            GivenTheUserSignsUpAgainWithTheSameInformation,
            WhenTheUserSignsInWithTheirPassword,
            ThenTheyAreSignedIn);
}
