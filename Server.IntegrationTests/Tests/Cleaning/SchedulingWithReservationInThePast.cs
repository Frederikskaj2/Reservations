using LightBDD.Framework;
using LightBDD.Framework.Scenarios;
using LightBDD.XUnit2;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Server.IntegrationTests.Tests.Cleaning;

[FeatureDescription(
    """
    As a system
    I want the cleaning schedule to be automatically updated even when reservations are in the past
    So the cleaning schedule always is up to date
    """)]
public partial class SchedulingWithReservationInThePast
{
    [Scenario]
    public Task ConfirmedReservationWithCleaningInThePast() =>
        Runner.RunScenarioAsync(
            GivenAResident,
            GivenAConfirmedReservation,
            GivenTheScheduledCleaningStartsInThePast,
            WhenTheCleaningScheduleIsRetrieved,
            ThenCleaningIsNotScheduled);

    [Scenario]
    public Task ConfirmedReservationWithCleaningInTheFuture() =>
        Runner.RunScenarioAsync(
            GivenAResident,
            GivenAConfirmedReservation,
            GivenTheScheduledCleaningStartsInTheFuture,
            WhenTheCleaningScheduleIsRetrieved,
            ThenCleaningIsScheduled);

    [Scenario]
    public Task OwnerReservationWithCleaningInThePast() =>
        Runner.RunScenarioAsync(
            GivenAnOwnerReservation,
            GivenTheScheduledCleaningStartsInThePast,
            WhenTheCleaningScheduleIsRetrieved,
            ThenCleaningIsNotScheduled);

    [Scenario]
    public Task OwnerReservationWithCleaningInTheFuture() =>
        Runner.RunScenarioAsync(
            GivenAnOwnerReservation,
            GivenTheScheduledCleaningStartsInTheFuture,
            WhenTheCleaningScheduleIsRetrieved,
            ThenCleaningIsScheduled);
}
