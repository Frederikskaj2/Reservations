using LightBDD.Framework;
using LightBDD.Framework.Scenarios;
using LightBDD.XUnit2;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Server.IntegrationTests.Tests.Orders;

[FeatureDescription(
    """
    As a an administrator
    I want to settle a reservation
    So that the resident's deposit can be refunded
    """)]
public partial class SettleReservation
{
    [Scenario]
    public Task ReservationIsSettled() =>
        Runner.RunScenarioAsync(
            GivenAResidentHasAPaidOrder,
            WhenTheReservationIsSettled,
            ThenTheOrderIsSettled,
            ThenTheResidentHasABalanceThatIsTheDepositedAmount,
            ThenTheDepositIsOwedToTheResident);

    [Scenario]
    public Task RefundedDepositIsUsedToConfirmAnUnpaidOrder() =>
        Runner.RunScenarioAsync(
            GivenAResidentHasAPaidOrder,
            GivenAnotherOrderIsPlaced,
            WhenTheReservationIsSettled,
            ThenTheOrderIsSettled,
            ThenTheOtherOrderIsConfirmed,
            ThenTheResidentHasABalanceThatIsTheDepositFromTheFirstMinusThePriceOfTheSecondOrder);

    [Scenario]
    public Task ReservationIsSettledWithDamages() =>
        Runner.RunScenarioAsync(
            GivenAResidentHasAPaidOrder,
            WhenTheReservationIsSettledWithDamages,
            ThenTheOrderIsSettled,
            ThenTheOrderHasADamagesLineItem,
            ThenTheResidentHasABalanceThatIsTheRemainingDeposit,
            ThenTheRemainingDepositIsOwedToTheResident);
}
