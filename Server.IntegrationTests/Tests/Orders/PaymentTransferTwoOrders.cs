using LightBDD.Framework;
using LightBDD.Framework.Scenarios;
using LightBDD.XUnit2;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Server.IntegrationTests.Tests.Orders;

[FeatureDescription(
    """
    As a resident
    I want to place and pay an order, cancel the order and place a new order that is partially paid by my refund of the first order
    So that my refunds are applied against my outstanding debt
    """)]
public partial class PaymentTransferTwoOrders
{
    [Scenario]
    public Task ResidentPlacesAnOrderThenCancelsItAndPlacesAnotherOrder() =>
        Runner.RunScenarioAsync(
            GivenAResidentHasPlacedAndPaidAnOrder,
            GivenTheResidentHasCancelledTheOrder,
            WhenTheResidentPlacesASimilarOrderAndPaysTheOutstandingAmount,
            ThenTheFirstOrderIsCancelled,
            ThenTheSecondOrderIsConfirmed,
            ThenTheAmountPaidTheSecondTimeIsTheCancellationFee,
            ThenTheResidentsBalanceIs0);
}
