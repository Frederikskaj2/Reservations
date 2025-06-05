using LightBDD.Framework;
using LightBDD.Framework.Scenarios;
using LightBDD.XUnit2;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Server.IntegrationTests.Tests.Users;

[FeatureDescription(
    """
    As a user
    I want to be able to sign out on all other devices
    So that I can stop malicious access to my account without having to update my password
    """)]
public partial class SignOut
{
    [Scenario]
    public Task SignOutEverywhereElse() =>
        Runner.RunScenarioAsync(
            GivenAUserSignsInOnDevice1,
            GivenAUserSignsInOnDevice2,
            WhenTheUserSignsOutEverywhereElse,
            ThenTheUserIsNoLongerAbleToAccessTheSystemFromDevice1);
}
