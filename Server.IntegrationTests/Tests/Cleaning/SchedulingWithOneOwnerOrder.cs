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
public partial class SchedulingWithOneOwnerOrder
{
    [Scenario]
    public Task OneReservationWithCleaning() =>
        Runner.RunScenarioAsync(
            GivenAnOrderWithOneReservationWithCleaning,
            WhenTheCleaningScheduleIsRetrieved,
            ThenCleaningIsScheduledAfterTheReservation);

    [Scenario]
    public Task OneReservationWithoutCleaning() =>
        Runner.RunScenarioAsync(
            GivenAnOrderWithOneReservationWithoutCleaning,
            WhenTheCleaningScheduleIsRetrieved,
            ThenNoCleaningIsScheduled);

    [Scenario]
    public Task OneReservationWithCleaningRemoved() =>
        Runner.RunScenarioAsync(
            GivenAnOrderWithOneReservationWithCleaning,
            GivenCleaningIsRemoved,
            WhenTheCleaningScheduleIsRetrieved,
            ThenNoCleaningIsScheduled);

    [Scenario]
    public Task OneReservationWithCleaningAdded() =>
        Runner.RunScenarioAsync(
            GivenAnOrderWithOneReservationWithoutCleaning,
            GivenCleaningIsAdded,
            WhenTheCleaningScheduleIsRetrieved,
            ThenCleaningIsScheduledAfterTheReservation);
}
