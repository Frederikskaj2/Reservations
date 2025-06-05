using LightBDD.Framework;
using LightBDD.Framework.Scenarios;
using LightBDD.XUnit2;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Server.IntegrationTests.Tests.Users;

[FeatureDescription(
    """
    As a user
    I want to be locked out if I sign in with an incorrect password too many times
    So that it will be challenging for someone else to guess my password by trial and error
    """)]
public partial class SignIn
{
    [Scenario]
    public Task SignInWithIncorrectPasswordShouldNotLockOut() =>
        Runner.RunScenarioAsync(
            GivenAUserSignsUp,
            GivenTheUserSignsInWithAnIncorrectPassword,
            WhenTheUserSignsInWithTheCorrectPassword,
            ThenTheUserIsSignedIn);

    [Scenario]
    public Task SignInWithIncorrectPasswordMultipleTimesShouldLockOut() =>
        Runner.RunScenarioAsync(
            GivenAUserSignsUp,
            GivenTheUserSignsInWithAnIncorrectPasswordTooManyTimes,
            WhenTheUserSignsInWithTheCorrectPassword,
            ThenTheUserIsNotSignedIn);

    [Scenario]
    public Task SignInWithIncorrectPasswordMultipleTimesAndThenWaitShouldRemoveLockOut() =>
        Runner.RunScenarioAsync(
            GivenAUserSignsUp,
            GivenTheUserSignsInWithAnIncorrectPasswordTooManyTimes,
            GivenTheUserWaitsForTheLockoutPeriodToExpire,
            WhenTheUserSignsInWithTheCorrectPassword,
            ThenTheUserIsSignedIn);

    [Scenario]
    public Task SignInOnceWithIncorrectPasswordAfterLockoutShouldNotLockOut() =>
        Runner.RunScenarioAsync(
            GivenAUserSignsUp,
            GivenTheUserSignsInWithAnIncorrectPasswordTooManyTimes,
            GivenTheUserWaitsForTheLockoutPeriodToExpire,
            GivenTheUserSignsInWithTheCorrectPassword,
            GivenTheUserSignsInWithAnIncorrectPassword,
            WhenTheUserSignsInWithTheCorrectPassword,
            ThenTheUserIsSignedIn);
}
