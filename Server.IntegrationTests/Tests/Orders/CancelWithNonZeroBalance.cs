using LightBDD.Framework;
using LightBDD.Framework.Scenarios;
using LightBDD.XUnit2;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Server.IntegrationTests.Tests.Orders;

[FeatureDescription(
    """
    As a resident
    I want to be able to create and cancel an order when I have a non-zero balance
    So that my balance is correct after I cancel the order
    """)]
public partial class CancelWithNonZeroBalance
{
    [Scenario]
    public Task ResidentCreatesAndCancelsUnpaidOrderWhenOwedMoney() =>
        Runner.RunScenarioAsync(
            GivenAResidentIsSignedIn,
            GivenResidentIsOwedMoney,
            GivenAnOrder,
            WhenTheOrderIsCancelled,
            ThenTheResidentsBalanceIsTheOwedAmount,
            ThenTheResidentIsACreditorForTheOwedAmount);

    [Scenario]
    public Task ResidentCreatesAndCancelsUnpaidOrderWhenOwingMoney() =>
        Runner.RunScenarioAsync(
            GivenAResidentIsSignedIn,
            GivenResidentOwesMoney,
            GivenAnOrder,
            WhenTheOrderIsCancelled,
            ThenTheResidentsBalanceIsTheAmountPreviouslyOwed,
            ThenTheResidentIsNotACreditor);

    [Scenario]
    public Task ResidentCreatesAndCancelsPaidOrderWhenOwedMoney() =>
        Runner.RunScenarioAsync(
            GivenAResidentIsSignedIn,
            GivenResidentIsOwedMoney,
            GivenAPaidOrder,
            WhenTheOrderIsCancelled,
            ThenTheOrderHasACancellationFee,
            ThenTheResidentsBalanceIsTheOwedAmountAndTheRefund,
            ThenTheResidentIsACreditorForTheOwedAmountAndTheRefund);
}
