using Frederikskaj2.Reservations.Users;
using LightBDD.Framework;
using LightBDD.Framework.Scenarios;
using LightBDD.XUnit2;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Server.IntegrationTests.Tests.Orders;

[FeatureDescription(
    """
    As a resident
    I want to be able to make reservations
    So that I can use the shared house
    """)]
public partial class ResidentOrder
{
    static readonly Amount fee = Amount.FromInt32(200);

    [Scenario]
    public Task ResidentPlacesAnOrder() =>
        Runner.RunScenarioAsync(
            _ => GivenAResidentIsSignedIn(),
            _ => WhenAnOrderIsPlaced(),
            _ => WhenTheJobToConfirmOrdersExecute(),
            _ => ThenTheResidentReceivesAnOrderReceivedEmail(),
            _ => ThenTheAdministratorReceivesANewOrderEmail(),
            _ => ThenTheOrderIsNotConfirmed(),
            _ => ThenTheResidentsBalanceIs(-Order.Price.Total()),
            _ => ThenTheResidentHasAnOutstandingPayment(Order.Price.Total()));

    [Scenario]
    public Task ResidentPlacesAndPaysAnOrder() =>
        Runner.RunScenarioAsync(
            _ => GivenAResidentIsSignedIn(),
            _ => WhenAnOrderIsPlaced(),
            _ => WhenTheOrderIsPaid(Order.Price.Total()),
            _ => WhenTheJobToConfirmOrdersExecute(),
            _ => ThenTheResidentReceivesAnOrderReceivedEmail(),
            _ => ThenTheAdministratorReceivesANewOrderEmail(),
            _ => ThenTheResidentReceivesAPayInEmail(Order.Price.Total()),
            _ => ThenTheOrderIsConfirmed(),
            _ => ThenTheResidentsBalanceIs(Amount.Zero),
            _ => ThenTheResidentHasNoOutstandingPayment());

    [Scenario]
    public Task ResidentPlacesAnOrderAndPaysTooLittle()
    {
        var missingAmount = Amount.FromInt32(100);
        return Runner.RunScenarioAsync(
            _ => GivenAResidentIsSignedIn(),
            _ => WhenAnOrderIsPlaced(),
            _ => WhenTheOrderIsPaid(Order.Price.Total() - missingAmount),
            _ => WhenTheJobToConfirmOrdersExecute(),
            _ => ThenTheResidentReceivesAnOrderReceivedEmail(),
            _ => ThenTheAdministratorReceivesANewOrderEmail(),
            _ => ThenTheResidentReceivesAPayInEmail(Order.Price.Total() - missingAmount),
            _ => ThenTheOrderIsNotConfirmed(),
            _ => ThenTheResidentsBalanceIs(-missingAmount),
            _ => ThenTheResidentHasAnOutstandingPayment(missingAmount));
    }

    [Scenario]
    public Task ResidentPlacesAnOrderAndPaysTooMuch()
    {
        var excessAmount = Amount.FromInt32(100);
        return Runner.RunScenarioAsync(
            _ => GivenAResidentIsSignedIn(),
            _ => WhenAnOrderIsPlaced(),
            _ => WhenTheOrderIsPaid(Order.Price.Total() + excessAmount),
            _ => WhenTheJobToConfirmOrdersExecute(),
            _ => ThenTheResidentReceivesAnOrderReceivedEmail(),
            _ => ThenTheAdministratorReceivesANewOrderEmail(),
            _ => ThenTheResidentReceivesAPayInEmail(Order.Price.Total() + excessAmount),
            _ => ThenTheOrderIsConfirmed(),
            _ => ThenTheResidentsBalanceIs(excessAmount),
            _ => ThenTheResidentHasNoOutstandingPayment());
    }

    [Scenario]
    public Task ResidentPlacesAnOrderAndPaysTwice()
    {
        var firstAmount = Amount.FromInt32(100);
        return Runner.RunScenarioAsync(
            _ => GivenAResidentIsSignedIn(),
            _ => WhenAnOrderIsPlaced(),
            _ => WhenTheOrderIsPaid(firstAmount),
            _ => WhenTheOrderIsPaid(Order.Price.Total() - firstAmount),
            _ => WhenTheJobToConfirmOrdersExecute(),
            _ => ThenTheOrderIsConfirmed(),
            _ => ThenTheResidentsBalanceIs(Amount.Zero),
            _ => ThenTheResidentHasNoOutstandingPayment());
    }

    [Scenario]
    public Task ResidentCancelsAnOrder() =>
        Runner.RunScenarioAsync(
            _ => GivenAResidentIsSignedIn(),
            _ => WhenAnOrderIsPlaced(),
            _ => WhenTheOrderIsCancelled(),
            _ => WhenTheJobToConfirmOrdersExecute(),
            _ => ThenTheResidentReceivesAReservationCancelledEmail(Amount.Zero, Amount.Zero),
            _ => ThenTheOrderIsAbandoned(),
            _ => ThenTheOrderHasNoCancellationFee(),
            _ => ThenTheResidentsBalanceIs(Amount.Zero),
            _ => ThenTheResidentHasNoOutstandingPayment(),
            _ => ThenTheOrderFinishedWithoutConfirmation(),
            _ => ThenTheOrderIsAbandonedWhenViewedByAnAdministrator(),
            _ => ThenTheOrderHasNoCancellationFeeWhenViewedByAnAdministrator());

    [Scenario]
    public Task ResidentCancelsAPaidOrder() =>
        Runner.RunScenarioAsync(
            _ => GivenAResidentIsSignedIn(),
            _ => WhenAnOrderIsPlaced(),
            _ => WhenTheOrderIsPaid(Order.Price.Total()),
            _ => WhenTheJobToConfirmOrdersExecute(),
            _ => WhenTheOrderIsCancelled(),
            _ => ThenTheResidentReceivesAReservationCancelledEmail(Order.Price.Total() - fee, fee),
            _ => ThenTheOrderIsCancelled(),
            _ => ThenTheOrderHasACancellationFee(),
            _ => ThenTheResidentsBalanceIs(Order.Price.Total() + CancellationFee),
            _ => ThenTheResidentHasNoOutstandingPayment(),
            _ => ThenTheOrderIsConfirmedAndFinished(),
            _ => ThenTheOrderIsCancelledWhenViewedByAnAdministrator(),
            _ => ThenTheOrderHasACancellationFeeWhenViewedByAnAdministrator());
}
