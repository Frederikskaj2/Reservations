using LightBDD.Framework;
using LightBDD.Framework.Scenarios;
using LightBDD.XUnit2;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Server.IntegrationTests.Tests.Orders;

[FeatureDescription(
    """
    As a resident
    I want to place and pay multiple orders, and after cancelling some paid orders, have unpaid orders paid by my refunds
    So that my refunds are applied against my outstanding debt
    """)]
public partial class PaymentTransferThreeOrders
{
    [Scenario]
    public Task ResidentPlacesAndPaysTwoOrdersThenPlacesAThirdOrderAndCancelsTheFirstTwoOrders() =>
        Runner.RunScenarioAsync(
            GivenAResidentHasPlacedAndPaidAnOrder,
            GivenTheResidentHasPlacedAndPaidAnotherOrder,
            GivenTheResidentHasPlacedButNotPaidYetAnotherOrder,
            WhenTheResidentCancelsTheFirstOrder,
            WhenTheResidentCancelsTheSecondOrder,
            ThenTheFirstOrderIsCancelled,
            ThenTheSecondOrderIsCancelled,
            ThenTheThirdOrderIsConfirmed,
            ThenTheResidentsBalanceIsTheRefundsMinusTheCancellationFeesMinusThePriceOfTheConfirmedOrder);

    [Scenario]
    public Task ResidentPlacesAndPaysAnOrderThenPlacesTwoMoreOrdersAndCancelsTheFirstOrder() =>
        Runner.RunScenarioAsync(
            GivenAResidentHasPlacedAndPaidAnOrder,
            GivenTheResidentHasPlacedButNotPaidAnotherOrder,
            GivenTheResidentHasPlacedButNotPaidYetAnotherOrder,
            WhenTheResidentCancelsTheFirstOrder,
            ThenTheFirstOrderIsCancelled,
            ThenTheSecondOrderIsConfirmed,
            ThenTheThirdOrderIsConfirmed,
            ThenTheResidentsBalanceIsTheRefundMinusTheCancellationFeeMinusThePriceOfTheConfirmedOrders);
}
