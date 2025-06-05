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
public partial class PaymentTransferFourOrders
{
    [Scenario]
    public Task ResidentPlacesAndPaysAnOrderThenPlacesThreeMoreOrdersAndCancelsTheFirstOrderConfirmingTheSecondOrder() =>
        Runner.RunScenarioAsync(
            GivenAResidentHasPlacedAndPaidAnOrderWithAMediumPrice,
            GivenTheResidentHasPlacedButNotPaidAnotherOrder,
            GivenTheResidentHasPlacedButNotPaidAThirdOrder,
            GivenTheResidentHasPlacedButNotPaidAFourthOrder,
            WhenTheResidentCancelsTheFirstOrder,
            ThenTheFirstOrderIsCancelled,
            ThenTheSecondOrderIsConfirmed,
            ThenTheThirdOrderIsNotConfirmed,
            ThenTheFourthOrderIsNotConfirmed,
            ThenTheResidentsBalanceIsTheRefundMinusTheCancellationFeeMinusThePriceOfTheOtherOrders);

    [Scenario]
    public Task ResidentPlacesAndPaysAnOrderThenPlacesThreeMoreOrdersAndCancelsTheFirstOrderConfirmingTwoOfTheThreeOrders() =>
        Runner.RunScenarioAsync(
            GivenAResidentHasPlacedAndPaidAnOrderWithAHighPrice,
            GivenTheResidentHasPlacedButNotPaidAnotherOrder,
            GivenTheResidentHasPlacedButNotPaidAThirdOrder,
            GivenTheResidentHasPlacedButNotPaidAFourthOrder,
            WhenTheResidentCancelsTheFirstOrder,
            ThenTheFirstOrderIsCancelled,
            ThenTheSecondOrderIsConfirmed,
            ThenTheThirdOrderIsConfirmed,
            ThenTheFourthOrderIsNotConfirmed,
            ThenTheResidentsBalanceIsTheRefundMinusTheCancellationFeeMinusThePriceOfTheOtherOrders);
}
