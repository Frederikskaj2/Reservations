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
public partial class SchedulingWithTwoOrders
{
    [Scenario]
    public Task TwoConfirmedReservations() =>
        Runner.RunScenarioAsync(
            GivenAResident,
            GivenAConfirmedReservation,
            GivenAnotherConfirmedReservation,
            WhenTheCleaningScheduleIsRetrieved,
            ThenCleaningIsScheduledBetweenTheReservations);

    [Scenario]
    public Task OneConfirmedAndOneReservedReservations() =>
        Runner.RunScenarioAsync(
            GivenAResident,
            GivenAConfirmedReservation,
            GivenAnotherReservedReservation,
            WhenTheCleaningScheduleIsRetrieved,
            ThenCleaningIsScheduledAfterTheFirstReservationIgnoringTheSecond);

    [Scenario]
    public Task OneOwnerReservationAnOneConfirmedReservation() =>
        Runner.RunScenarioAsync(
            GivenAResident,
            GivenAnOwnerReservation,
            GivenAnotherConfirmedReservation,
            WhenTheCleaningScheduleIsRetrieved,
            ThenCleaningIsScheduledBetweenTheReservations);

    [Scenario]
    public Task TwoOwnerReservations() =>
        Runner.RunScenarioAsync(
            GivenAnOwnerReservation,
            GivenAnotherOwnerReservation,
            WhenTheCleaningScheduleIsRetrieved,
            ThenCleaningIsScheduledBetweenTheReservations);
}
