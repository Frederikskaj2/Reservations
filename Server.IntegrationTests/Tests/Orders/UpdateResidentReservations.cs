using LightBDD.Framework;
using LightBDD.Framework.Scenarios;
using LightBDD.XUnit2;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Server.IntegrationTests.Tests.Orders;

[FeatureDescription(
    """
    As a an administrator
    I want to be able to update the date and duration of a resident's reservation
    So that the resident is able have an existing order adjusted without creating a brand new order
    """)]
public partial class UpdateResidentReservations
{
    [Scenario]
    public Task UpdateReservations() =>
        Runner.RunScenarioAsync(
            GivenAConfirmedResidentOrder,
            WhenAnAdministratorUpdatesTheReservations,
            ThenTheUpdateSucceeds,
            ThenTheResidentsOrderIsUpdated,
            ThenCalendarIsUpdated,
            ThenTheResidentsBalanceIsUpdated,
            ThenTheUpdateIsAudited);

    [Scenario]
    public Task CannotMakeConflictingReservationUpdate() =>
        Runner.RunScenarioAsync(
            GivenAConfirmedResidentOrder,
            GivenAnotherConfirmedResidentOrderRightAfterTheFirst,
            WhenAnAdministratorUpdatesTheReservations,
            ThenTheUpdateFailsWithConflict);
}
