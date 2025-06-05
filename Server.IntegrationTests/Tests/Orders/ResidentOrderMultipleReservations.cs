using Frederikskaj2.Reservations.Users;
using LightBDD.Framework;
using LightBDD.Framework.Scenarios;
using LightBDD.XUnit2;
using System.Linq;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Server.IntegrationTests.Tests.Orders;

[FeatureDescription(
    """
    As a resident
    I want to be able to cancel reservations on order
    So that I can get some of my money back
    """)]
public partial class ResidentOrderMultipleReservations
{
    [Scenario]
    public Task ResidentCancelsOneOfMultipleReservationsOnUnpaidOrder() =>
        Runner.RunScenarioAsync(
            _ => GivenAResidentIsSignedIn(),
            _ => GivenAnOrderIsPlaced(),
            _ => WhenAReservationsIsCancelled(),
            _ => ThenOneReservationIsAbandonedAndTheOtherReserved(),
            _ => ThenTheOrderHasNoCancellationFee(),
            _ => ThenTheResidentReceivesAReservationCancelledEmail(Amount.Zero, Amount.Zero),
            _ => ThenOneReservationIsAbandonedAndTheOtherReservedWhenViewedByAnAdministrator(),
            _ => ThenTheResidentsBalanceIsThePriceOfTheOtherReservation());

    [Scenario]
    public Task ResidentCancelsOneOfMultipleReservationsOnPaidOrder() =>
        Runner.RunScenarioAsync(
            _ => GivenAResidentIsSignedIn(),
            _ => GivenAnOrderIsPlaced(),
            _ => GivenTheOrderIsPaid(),
            _ => WhenAReservationsIsCancelled(),
            _ => ThenOneReservationIsCancelledAndTheOtherConfirmed(),
            _ => ThenTheOrderHasACancellationFee(),
            _ => ThenTheResidentReceivesAReservationCancelledEmail(Order.Reservations.First().Price!.Total() + CancellationFee, -CancellationFee),
            _ => ThenOneReservationIsCancelledAndTheOtherConfirmedWhenViewedByAnAdministrator(),
            _ => ThenTheResidentsBalanceIsThePriceOfTheReservationMinusTheCancellationFee());
}
