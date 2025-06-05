using LightBDD.Framework;
using LightBDD.Framework.Scenarios;
using LightBDD.XUnit2;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Server.IntegrationTests.Tests.Users;

[FeatureDescription(
    """
    As a client application
    I want to create an access token
    So that I can use the API
    """)]
public partial class AccessTokenCreation
{
    [Scenario]
    public Task CreateAccessToken() =>
        Runner.RunScenarioAsync(
            GivenAClientIsSignedIn,
            WhenAnAccessTokenIsCreated,
            ThenTheAccessTokenShouldHaveAValue);

    [Scenario]
    public Task CreateAccessTokenWithExpiredRefreshToken() =>
        Runner.RunScenarioAsync(
            GivenAClientIsSignedIn,
            GivenTheRefreshTokenHasExpired,
            WhenAnAccessTokenIsCreated1,
            ThenAccessIsForbidden);

    [Scenario]
    public Task CreateAccessTokenWithInvalidRefreshToken() =>
        Runner.RunScenarioAsync(
            GivenAClientIsSignedIn,
            GivenTheRefreshTokenIsInvalid,
            WhenAnAccessTokenIsCreatedUsingTheInvalidRefreshToken,
            ThenAccessIsForbidden);

    [Scenario]
    public Task CreateMultipleAccessTokens() =>
        Runner.RunScenarioAsync(
            GivenAClientIsSignedIn,
            GivenTheClientSignsInAgain,
            WhenAnAccessTokenIsCreatedUsingTheCookieFromTheFirstSignIn,
            ThenTheAccessTokenShouldHaveAValue);
}
