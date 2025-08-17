using LightBDD.Framework;
using LightBDD.Framework.Scenarios;
using LightBDD.XUnit2;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Server.IntegrationTests.Tests.Orders;

[FeatureDescription(
    """
    As a reservation system
    I want to ensure that only administrators and not residents can cancel reservations near the reservation date
    So that the cleaning can be scheduled in advance
    """)]
public partial class CancelShortlyBeforeReservationDate
{
    [Scenario]
    public Task ResidentCannotCancel() =>
        Runner.RunScenarioAsync(
            GivenAConfirmedOrder,
            GivenTheDeadlineForResidentCancellationIsInThePast,
            WhenTheResidentCancelsTheReservation,
            ThenTheReservationCannotBeCancelledByTheResident,
            ThenTheRequestToCancelTheReservationFails);

    [Scenario]
    public Task AdministratorCanCanCancel() =>
        Runner.RunScenarioAsync(
            GivenAConfirmedOrder,
            GivenTheDeadlineForResidentCancellationIsInThePast,
            WhenTheAdministratorCancelsTheReservation,
            ThenTheRequestToCancelTheReservationIsSuccessful,
            ThenTheReservationIsCancelled);

    [Scenario]
    public Task ResidentCanCanCancelWhenAllowed() =>
        Runner.RunScenarioAsync(
            GivenAConfirmedOrder,
            GivenTheDeadlineForResidentCancellationIsInThePast,
            GivenTheResidentIsAllowedToCancelWithoutFee,
            WhenTheResidentCancelsTheReservation,
            ThenTheRequestToCancelTheReservationIsSuccessful,
            ThenTheReservationIsCancelled);
}
