using LightBDD.Framework;
using LightBDD.Framework.Scenarios;
using LightBDD.XUnit2;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Server.IntegrationTests.Tests.Users;

[FeatureDescription(
    """
    As a user
    I want to be able to update my information
    So that I can ensure my personal details are correctly registered in the system
    """)]
public partial class UpdateUser
{
    [Scenario]
    public Task UserUpdatesTheirName() =>
        Runner.RunScenarioAsync(
            GivenASignedInUser,
            WhenTheUserUpdatesTheirName,
            ThenTheNameOfTheUserIsUpdated,
            ThenTheChangeOfTheNameIsAudited);

    [Scenario]
    public Task UserUpdatesTheirPhoneNumber() =>
        Runner.RunScenarioAsync(
            GivenASignedInUser,
            WhenTheUserUpdatesTheirPhoneNumber,
            ThenThePhoneNumberOfTheUserIsUpdated,
            ThenTheChangeOfThePhoneNumberIsAudited);

    [Scenario]
    public Task ResidentTriesToUpdateTheirEmailSubscriptions() =>
        Runner.RunScenarioAsync(
            GivenASignedInResident,
            WhenTheUserUpdatesTheirEmailSubscriptions,
            ThenTheRequestFails);

    [Scenario]
    public Task ResidentThatIsAlsoAdministratorUpdatesTheirEmailSubscriptions() =>
        Runner.RunScenarioAsync(
            GivenASignedInUser,
            GivenTheUserIsBothAResidentAndAnAdministrator,
            WhenTheUserUpdatesTheirEmailSubscriptions,
            ThenTheRequestSucceeds);

    [Scenario]
    public Task AdministratorUpdatesTheirEmailSubscriptions() =>
        Runner.RunScenarioAsync(
            GivenASignedInResident,
            GivenTheUserIsAnAdministrator,
            WhenTheUserUpdatesTheirEmailSubscriptions,
            ThenTheRequestSucceeds);
}
