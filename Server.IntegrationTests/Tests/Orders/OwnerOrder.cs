using LightBDD.Framework;
using LightBDD.Framework.Scenarios;
using LightBDD.XUnit2;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Server.IntegrationTests.Tests.Orders;

[FeatureDescription(
    """
    As an owner
    I want to be able to make reservations
    So that I can use the shared house without having to pay
    """)]
public partial class OwnerOrder
{
    [Scenario]
    public Task PlaceOrder() =>
        Runner.RunScenarioAsync(
            GivenAnOwnerOrder,
            WhenTheJobToFinishOwnerOrdersHasExecuted,
            ThenTheOrderIsAnOwnerOrder,
            ThenTheReservationIsConfirmed,
            ThenTheOwnerOrdersContainTheOrder,
            ThenTheOrderIsReservedInTheCalendar,
            ThenCleaningIsRequired,
            ThenCleaningIsScheduled,
            ThenTheOrderPlacementIsAudited,
            ThenTheOrderPlacementIsAuditedForTheUser);

    [Scenario]
    public Task CancelOrder() =>
        Runner.RunScenarioAsync(
            GivenAnOwnerOrder,
            WhenTheReservationIsCancelled,
            WhenTheJobToFinishOwnerOrdersHasExecuted,
            ThenTheOrderIsAnOwnerOrder,
            ThenTheReservationIsCancelled,
            ThenTheOwnerOrdersDoesNotContainTheOrder,
            ThenTheOrderIsNotReservedInTheCalendar,
            ThenNoCleaningIsScheduled,
            ThenTheOrderCancellationIsAudited);

    [Scenario]
    public Task CancelOneReservation() =>
        Runner.RunScenarioAsync(
            GivenAnOwnerOrderWithTwoReservations,
            WhenTheReservationIsCancelled,
            WhenTheJobToFinishOwnerOrdersHasExecuted,
            ThenTheOrderIsAnOwnerOrder,
            ThenTheOneReservationIsCancelledAndOneConfirmed,
            ThenTheOwnerOrdersContainTheOrder,
            ThenTheOrderIsReservedInTheCalendar,
            ThenNoCleaningIsScheduled);

    [Scenario]
    public Task UpdateDescription() =>
        Runner.RunScenarioAsync(
            GivenAnOwnerOrder,
            WhenTheDescriptionIsUpdated,
            WhenTheJobToFinishOwnerOrdersHasExecuted,
            ThenTheDescriptionIsUpdated,
            ThenTheDescriptionUpdateIsAudited);

    [Scenario]
    public Task CancelOneReservationAndUpdateDescription() =>
        Runner.RunScenarioAsync(
            GivenAnOwnerOrderWithTwoReservations,
            WhenTheReservationIsCancelled,
            WhenTheDescriptionIsUpdated,
            WhenTheJobToFinishOwnerOrdersHasExecuted,
            ThenTheOneReservationIsCancelledAndOneConfirmed,
            ThenTheDescriptionIsUpdated,
            ThenTheOwnerOrdersContainTheOrder,
            ThenTheOrderIsReservedInTheCalendar,
            ThenTheCancellationAndDescriptionUpdateAreAudited);

    [Scenario]
    public Task ChangeCleaning() =>
        Runner.RunScenarioAsync(
            GivenAnOwnerOrder,
            WhenCleaningIsNoLongerRequired,
            WhenTheJobToFinishOwnerOrdersHasExecuted,
            ThenCleaningIsNotRequired,
            ThenNoCleaningIsScheduled,
            ThenTheCleaningUpdateIsAudited);
}
