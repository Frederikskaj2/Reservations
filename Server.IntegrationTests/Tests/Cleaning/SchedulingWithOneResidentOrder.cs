using LightBDD.Framework;
using LightBDD.Framework.Scenarios;
using LightBDD.XUnit2;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Server.IntegrationTests.Tests.Cleaning;

[FeatureDescription(
    """
    As a system
    I want the cleaning schedule to be automatically updated when orders are placed
    So the cleaning schedule always is up to date
    """)]
public partial class SchedulingWithOneResidentOrder
{
    [Scenario]
    public Task OneConfirmedReservation() =>
        Runner.RunScenarioAsync(
            GivenAnOrderWithOneConfirmedReservation,
            WhenTheCleaningScheduleIsRetrieved,
            ThenCleaningIsScheduledAfterTheReservation);

    [Scenario]
    public Task TwoConfirmedReservations() =>
        Runner.RunScenarioAsync(
            GivenAnOrderWithTwoConfirmedReservations,
            WhenTheCleaningScheduleIsRetrieved,
            ThenCleaningIsScheduledBetweenTheReservations);

    [Scenario]
    public Task OneConfirmedAndOneCancelledReservation() =>
        Runner.RunScenarioAsync(
            GivenAnOrderWithOneConfirmedAndOneCancelledReservation,
            WhenTheCleaningScheduleIsRetrieved,
            ThenCleaningIsScheduledAfterTheFirstReservationIgnoringTheSecond);
}
