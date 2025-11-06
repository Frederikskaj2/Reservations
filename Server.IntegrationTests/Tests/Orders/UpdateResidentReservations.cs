using LightBDD.Framework;
using LightBDD.Framework.Scenarios;
using LightBDD.XUnit2;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Server.IntegrationTests.Tests.Orders;

[FeatureDescription(
    """
    As an administrator
    I want to be able to update the date and duration of a resident's reservation
    So that the resident is able to have an existing order adjusted without creating a brand-new order
    """)]
public partial class UpdateResidentReservations
{
    [Scenario]
    public Task ExtendReservations() =>
        Runner.RunScenarioAsync(
            _ => GivenAConfirmedResidentOrder(),
            _ => WhenAnAdministratorUpdatesTheReservations(2, 1),
            _ => ThenTheUpdateSucceeds(),
            _ => ThenTheResidentsOrderIsUpdated(2, 1),
            _ => ThenCalendarIsUpdated(2, 1),
            _ => ThenTheResidentsBalanceIsNegative(),
            _ => ThenTheUpdateIsAudited());

    [Scenario]
    public Task ShortenReservations() =>
        Runner.RunScenarioAsync(
            _ => GivenAConfirmedResidentOrder(),
            _ => WhenAnAdministratorUpdatesTheReservations(-2, -1),
            _ => ThenTheUpdateSucceeds(),
            _ => ThenTheResidentsOrderIsUpdated(-2, -1),
            _ => ThenCalendarIsUpdated(-2, -1),
            _ => ThenTheResidentsBalanceIsPositive(),
            _ => ThenTheUpdateIsAudited());

    [Scenario]
    public Task CannotMakeConflictingReservationUpdate() =>
        Runner.RunScenarioAsync(
            _ => GivenAConfirmedResidentOrder(),
            _ => GivenAnotherConfirmedResidentOrderRightAfterTheFirst(),
            _ => WhenAnAdministratorUpdatesTheReservations(2, 1),
            _ => ThenTheUpdateFailsWithConflict());
}
