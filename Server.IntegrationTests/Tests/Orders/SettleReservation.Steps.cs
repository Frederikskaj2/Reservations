using FluentAssertions;
using Frederikskaj2.Reservations.Orders;
using Frederikskaj2.Reservations.Server.IntegrationTests.Harness;
using Frederikskaj2.Reservations.Users;
using LightBDD.Core.Extensibility.Execution;
using LightBDD.Framework;
using LightBDD.XUnit2;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Frederikskaj2.Reservations.Server.IntegrationTests.Tests.Orders;

sealed partial class SettleReservation(SessionFixture session) : FeatureFixture, IScenarioSetUp, IClassFixture<SessionFixture>
{
    const string description = "Description of damages";

    readonly Amount remainingAmount = Amount.FromInt32(100);

    State<MyOrderDto> order;
    State<MyOrderDto> anotherOrder;
    State<Amount> damages;

    MyOrderDto Order => order.GetValue(nameof(Order));
    MyOrderDto AnotherOrder => anotherOrder.GetValue(nameof(AnotherOrder));
    Amount Damages => damages.GetValue(nameof(Damages));

    async Task IScenarioSetUp.OnScenarioSetUp() => await session.UpdateLockBoxCodes();

    async Task GivenAResidentHasAPaidOrder()
    {
        await session.SignUpAndSignIn();
        var getMyOrderResponse = await session.PlaceAndPayResidentOrder(new TestReservation(SeedData.BanquetFacilities.ResourceId, 2));
        order = getMyOrderResponse.Order;
        await session.RunConfirmOrders();
    }

    async Task GivenAnotherOrderIsPlaced()
    {
        var placeMyOrderResponse = await session.PlaceResidentOrder(new TestReservation(SeedData.Frederik.ResourceId, 1, PriceGroup.Low));
        anotherOrder = placeMyOrderResponse.Order;
    }

    async Task WhenTheReservationIsSettled()
    {
        await session.SettleReservation(Order.OrderId, 0);
        await session.RunConfirmOrders();
    }

    async Task WhenTheReservationIsSettledWithDamages()
    {
        damages = Order.Reservations.Single().Price!.Deposit - remainingAmount;
        await session.SettleReservation(Order.OrderId, 0, damages, description);
    }

    async Task ThenTheOrderIsSettled()
    {
        var getOrderResponse = await session.GetOrder(Order.OrderId);
        getOrderResponse.Order.Should().NotBeNull();
        getOrderResponse.Order.Type.Should().Be(OrderType.Resident);
        getOrderResponse.Order.IsHistoryOrder.Should().BeTrue();
        getOrderResponse.Order.Reservations.Single().Status.Should().Be(ReservationStatus.Settled);
        getOrderResponse.Order.Audits.Select(audit => audit.Type).Should().Equal(
            OrderAuditType.PlaceOrder, OrderAuditType.ConfirmOrder, OrderAuditType.SettleReservation, OrderAuditType.FinishOrder);
    }

    async Task ThenTheOtherOrderIsConfirmed()
    {
        var getOrderResponse = await session.GetOrder(AnotherOrder.OrderId);
        getOrderResponse.Order.IsHistoryOrder.Should().BeFalse();
        getOrderResponse.Order.Reservations.Single().Status.Should().Be(ReservationStatus.Confirmed);
        getOrderResponse.Order.Audits.Select(audit => audit.Type).Should().Equal(OrderAuditType.PlaceOrder, OrderAuditType.ConfirmOrder);
    }

    async Task ThenTheOrderHasADamagesLineItem()
    {
        var getOrderResponse = await session.GetOrder(Order.OrderId);
        var damagesLineItem = getOrderResponse.Order.Resident!.AdditionalLineItems.Single();
        damagesLineItem.Type.Should().Be(LineItemType.Damages);
        damagesLineItem.Amount.Should().Be(-Damages);
        damagesLineItem.Damages!.Reservation.Should().Be(0);
        damagesLineItem.Damages.Description.Should().Be(description);
    }

    async Task ThenTheResidentHasABalanceThatIsTheDepositedAmount()
    {
        var getMyTransactionsResponse = await session.GetMyTransactions();
        var balance = getMyTransactionsResponse.Transactions.Select(transaction => transaction.Amount).Sum();
        balance.Should().Be(Order.Price.Deposit);
    }

    async Task ThenTheResidentHasABalanceThatIsTheDepositFromTheFirstMinusThePriceOfTheSecondOrder()
    {
        var getMyTransactionsResponse = await session.GetMyTransactions();
        var balance = getMyTransactionsResponse.Transactions.Select(transaction => transaction.Amount).Sum();
        var order1Deposit = Order.Price.Deposit;
        var order2Price = AnotherOrder.Price.Total();
        balance.Should().Be(order1Deposit - order2Price);
    }

    async Task ThenTheResidentHasABalanceThatIsTheRemainingDeposit()
    {
        var getMyTransactionsResponse = await session.GetMyTransactions();
        var balance = getMyTransactionsResponse.Transactions.Select(transaction => transaction.Amount).Sum();
        balance.Should().Be(remainingAmount);
    }

    async Task ThenTheDepositIsOwedToTheResident()
    {
        var getCreditorsResponse = await session.GetCreditors();
        var creditor = getCreditorsResponse.Creditors.SingleOrDefault(dto => dto.UserInformation.UserId == session.UserId());
        creditor.Should().NotBeNull();
        creditor.Payment.Amount.Should().Be(Order.Price.Deposit);
        creditor.Payment.AccountNumber.Should().NotBeEmpty();
    }

    async Task ThenTheRemainingDepositIsOwedToTheResident()
    {
        var getCreditorsResponse = await session.GetCreditors();
        var creditor = getCreditorsResponse.Creditors.SingleOrDefault(dto => dto.UserInformation.UserId == session.UserId());
        creditor.Should().NotBeNull();
        creditor.Payment.Amount.Should().Be(remainingAmount);
        creditor.Payment.AccountNumber.Should().NotBeEmpty();
    }
}
