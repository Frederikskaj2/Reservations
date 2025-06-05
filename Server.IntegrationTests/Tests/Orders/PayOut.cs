using Frederikskaj2.Reservations.Users;
using LightBDD.Framework;
using LightBDD.Framework.Scenarios;
using LightBDD.XUnit2;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Server.IntegrationTests.Tests.Orders;

[FeatureDescription(
    """
    As an administrator
    I want to be able to make pay-outs
    So that I can refund deposits owed to residents
    """)]
public partial class PayOut
{
    [Scenario]
    public Task FullyRefundDepositOnASingleOrder() =>
        Runner.RunScenarioAsync(
            _ => GivenAResident(),
            _ => GivenAPaidOrder(),
            _ => GivenTheOrderIsSettled(),
            _ => WhenTheDepositIsRefunded(Amount.Zero),
            _ => ThenTheCreditorHasAmount(Order1.Price.Deposit),
            _ => ThenTheResidentIsNoLongerACreditor());

    [Scenario]
    public Task PartiallyRefundDepositOnASingleOrder()
    {
        var missingAmount = Amount.FromInt32(100);
        return Runner.RunScenarioAsync(
            _ => GivenAResident(),
            _ => GivenAPaidOrder(),
            _ => GivenTheOrderIsSettled(),
            _ => WhenTheDepositIsRefunded(missingAmount),
            _ => ThenTheCreditorHasAmount(Order1.Price.Deposit),
            _ => ThenThePaidCreditorHasAmount(missingAmount),
            _ => ThenTheResidentsBalanceIsAmount(missingAmount));
    }

    [Scenario]
    public Task RefundDepositsOnTwoOrders() =>
        Runner.RunScenarioAsync(
            _ => GivenAResident(),
            _ => GivenAPaidOrder(),
            _ => GivenAnotherPaidOrder(),
            _ => GivenTheOrderIsSettled(),
            _ => GivenTheOtherOrderIsSettled(),
            _ => WhenTheDepositIsRefunded(Amount.Zero),
            _ => ThenTheCreditorHasAmount(Order1.Price.Deposit + Order2.Price.Deposit),
            _ => ThenTheResidentIsNoLongerACreditor());
}
