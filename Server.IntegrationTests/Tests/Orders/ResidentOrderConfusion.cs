using LightBDD.Framework;
using LightBDD.Framework.Scenarios;
using LightBDD.XUnit2;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Server.IntegrationTests.Tests.Orders;

[FeatureDescription(
    """
    As a resident
    I want to pay an order and try to change it by placing another order and then cancel everything
    So that I get my money back despite my confusion about how to change an order
    """)]
public partial class ResidentOrderConfusion
{
    [Scenario]
    public Task ResidentRegretsOrdering() =>
        Runner.RunScenarioAsync(
            GivenAResidentIsSignedIn,
            GivenAnOrderIsPlacedAndPaid,
            GivenAnotherOrderIsPlaced,
            WhenTheJobToConfirmOrdersExecute,
            WhenTheFirstOrderIsCancelled,
            WhenTheSecondOrderIsCancelled,
            ThenTheFirstOrderIsCancelled,
            ThenTheSecondOrderIsAbandoned,
            ThenTheFirstOrderHasACancellationFee,
            ThenTheSecondOrderHasNoCancellationFee,
            ThenTheResidentsBalanceIsThePaidPriceMinusTheCancellationFee);
}
