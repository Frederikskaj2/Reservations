using LightBDD.Framework;
using LightBDD.Framework.Scenarios;
using LightBDD.XUnit2;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Server.IntegrationTests.Tests.Orders;

[FeatureDescription(
    """
    As a system
    I want to change owner orders to history orders as time passes
    So that these orders become history orders automatically
    """)]
public partial class HistoryOwnerOrder
{
    [Scenario]
    public Task OwnerOrderBecomesHistoryOrder() =>
        Runner.RunScenarioAsync(
            GivenAnOwnerOrder,
            GivenTheOrderIsInThePast,
            WhenTheJobToFinishOwnerOrdersHasExecuted,
            ThenTheOrderBecomesAHistoryOrder);

    [Scenario]
    public Task OnlyOwnerOrderInThePastBecomesHistoryOrder() =>
        Runner.RunScenarioAsync(
            GivenAnOwnerOrder,
            GivenSecondOwnerOrderAtALaterTime,
            GivenTheOrderIsInThePast,
            WhenTheJobToFinishOwnerOrdersHasExecuted,
            ThenTheOrderBecomesAHistoryOrder,
            ThenTheSecondOrderIsNotAHistoryOrder);

    [Scenario]
    public Task OwnerOrderDoesNotBecomesHistoryWhenOneReservationIsStillInTheFuture() =>
        Runner.RunScenarioAsync(
            GivenAnOwnerOrderWithTwoReservations,
            GivenTheFirstReservationIsInThePast,
            WhenTheJobToFinishOwnerOrdersHasExecuted,
            ThenTheOrderIsNotAHistoryOrder);

    [Scenario]
    public Task OwnerOrderBecomesHistoryWhenAllReservationsAreInThePast() =>
        Runner.RunScenarioAsync(
            GivenAnOwnerOrderWithTwoReservations,
            GivenBothReservationsAreInThePast,
            WhenTheJobToFinishOwnerOrdersHasExecuted,
            ThenTheOrderBecomesAHistoryOrder);
}
