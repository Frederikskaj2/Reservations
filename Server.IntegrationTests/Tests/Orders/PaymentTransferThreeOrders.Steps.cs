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

sealed partial class PaymentTransferThreeOrders(SessionFixture session) : FeatureFixture, IScenarioSetUp, IClassFixture<SessionFixture>
{
    State<MyOrderDto> order1;
    State<MyOrderDto> order2;
    State<MyOrderDto> order3;
    State<MyOrderDto> cancelledOrder1;
    State<MyOrderDto> cancelledOrder2;

    MyOrderDto Order1 => order1.GetValue(nameof(Order1));
    MyOrderDto Order2 => order2.GetValue(nameof(Order2));
    MyOrderDto Order3 => order3.GetValue(nameof(Order3));
    MyOrderDto CancelledOrder1 => cancelledOrder1.GetValue(nameof(CancelledOrder1));
    MyOrderDto CancelledOrder2 => cancelledOrder2.GetValue(nameof(CancelledOrder2));

    async Task IScenarioSetUp.OnScenarioSetUp() => await session.UpdateLockBoxCodes();

    async Task GivenAResidentHasPlacedAndPaidAnOrder()
    {
        await session.SignUpAndSignIn();
        var getMyOrderResponse = await session.PlaceAndPayResidentOrder(new TestReservation(SeedData.Frederik.ResourceId, 7));
        order1 = getMyOrderResponse.Order;
        await session.RunConfirmOrders();
    }

    async Task GivenTheResidentHasPlacedAndPaidAnotherOrder()
    {
        var getMyOrderResponse = await session.PlaceAndPayResidentOrder(new TestReservation(SeedData.Kaj.ResourceId, 1, PriceGroup.Low));
        order2 = getMyOrderResponse.Order;
        await session.RunConfirmOrders();
    }

    async Task GivenTheResidentHasPlacedButNotPaidAnotherOrder()
    {
        var getMyOrderResponse = await session.PlaceResidentOrder(new TestReservation(SeedData.Frederik.ResourceId, 1, PriceGroup.Low));
        order2 = getMyOrderResponse.Order;
    }

    async Task GivenTheResidentHasPlacedButNotPaidYetAnotherOrder()
    {
        var placeMyOrderResponse = await session.PlaceResidentOrder(new TestReservation(SeedData.Frederik.ResourceId, 1, PriceGroup.Low));
        order3 = placeMyOrderResponse.Order;
    }

    async Task WhenTheResidentCancelsTheFirstOrder()
    {
        var updateMyOrderResponse = await session.CancelResidentReservations(Order1.OrderId, 0);
        cancelledOrder1 = updateMyOrderResponse.Order;
        await session.RunConfirmOrders();
    }

    async Task WhenTheResidentCancelsTheSecondOrder()
    {
        var updateMyOrderResponse = await session.CancelResidentReservations(Order2.OrderId, 0);
        cancelledOrder2 = updateMyOrderResponse.Order;
        await session.RunConfirmOrders();
    }

    async Task ThenTheFirstOrderIsCancelled()
    {
        var getOrderResponse = await session.GetOrder(Order1.OrderId);
        getOrderResponse.Order.IsHistoryOrder.Should().BeTrue();
        getOrderResponse.Order.Reservations.Single().Status.Should().Be(ReservationStatus.Cancelled);
        getOrderResponse.Order.Audits.Select(audit => audit.Type).Should().Equal(
            OrderAuditType.PlaceOrder, OrderAuditType.ConfirmOrder, OrderAuditType.CancelReservation, OrderAuditType.FinishOrder);
    }

    async Task ThenTheSecondOrderIsCancelled()
    {
        var getOrderResponse = await session.GetOrder(Order2.OrderId);
        getOrderResponse.Order.IsHistoryOrder.Should().BeTrue();
        getOrderResponse.Order.Reservations.Single().Status.Should().Be(ReservationStatus.Cancelled);
        getOrderResponse.Order.Audits.Select(audit => audit.Type).Should().Equal(
            OrderAuditType.PlaceOrder, OrderAuditType.ConfirmOrder, OrderAuditType.CancelReservation, OrderAuditType.FinishOrder);
    }

    async Task ThenTheSecondOrderIsConfirmed()
    {
        var getOrderResponse = await session.GetOrder(Order2.OrderId);
        getOrderResponse.Order.IsHistoryOrder.Should().BeFalse();
        getOrderResponse.Order.Reservations.Single().Status.Should().Be(ReservationStatus.Confirmed);
        getOrderResponse.Order.Audits.Select(audit => audit.Type).Should().Equal(OrderAuditType.PlaceOrder, OrderAuditType.ConfirmOrder);
    }

    async Task ThenTheThirdOrderIsConfirmed()
    {
        var getOrderResponse = await session.GetOrder(Order3.OrderId);
        getOrderResponse.Order.IsHistoryOrder.Should().BeFalse();
        getOrderResponse.Order.Reservations.Single().Status.Should().Be(ReservationStatus.Confirmed);
        getOrderResponse.Order.Audits.Select(audit => audit.Type).Should().Equal(OrderAuditType.PlaceOrder, OrderAuditType.ConfirmOrder);
    }

    async Task ThenTheResidentsBalanceIsTheRefundsMinusTheCancellationFeesMinusThePriceOfTheConfirmedOrder()
    {
        var getMyTransactionsResponse = await session.GetMyTransactions();
        var balance = getMyTransactionsResponse.Transactions.Select(transaction => transaction.Amount).Sum();
        var price1 = Order1.Price.Total();
        var price2 = Order2.Price.Total();
        var price3 = Order3.Price.Total();
        var cancellationFee1 = CancelledOrder1.AdditionalLineItems.Single().Amount;
        var cancellationFee2 = CancelledOrder2.AdditionalLineItems.Single().Amount;
        balance.Should().Be(price1 + price2 + cancellationFee1 + cancellationFee2 - price3);
    }

    async Task ThenTheResidentsBalanceIsTheRefundMinusTheCancellationFeeMinusThePriceOfTheConfirmedOrders()
    {
        var getMyTransactionsResponse = await session.GetMyTransactions();
        var balance = getMyTransactionsResponse.Transactions.Select(transaction => transaction.Amount).Sum();
        var price1 = Order1.Price.Total();
        var price2 = Order2.Price.Total();
        var price3 = Order3.Price.Total();
        var cancellationFee1 = CancelledOrder1.AdditionalLineItems.Single().Amount;
        balance.Should().Be(price1 + cancellationFee1 - price2 - price3);
    }
}
