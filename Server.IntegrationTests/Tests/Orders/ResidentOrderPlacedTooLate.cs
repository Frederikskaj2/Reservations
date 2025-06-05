using LightBDD.Framework;
using LightBDD.Framework.Scenarios;
using LightBDD.XUnit2;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Server.IntegrationTests.Tests.Orders;

[FeatureDescription(
    """
    As an administrator
    I want to be able to make reservations on behalf of a resident
    So that the resident can create an reservation after the deadline with some assistance
    """)]
public partial class ResidentOrderPlacedTooLate
{
    [Scenario]
    public Task AdministratorPlacesALateResidentOrder() =>
        Runner.RunScenarioAsync(
            GivenAResident,
            WhenAnAdministratorPlacesALateOrderForTheResident,
            ThenTheOrderIsPlaced,
            ThenTheOrderPlacementIsAuditedForTheAdministrator);

    [Scenario]
    public Task ResidentPlacesALateOrder() =>
        Runner.RunScenarioAsync(
            GivenAResident,
            WhenTheResidentPlacesALateOrder,
            ThenTheRequestIsDenied);
}
