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

sealed partial class PaymentTransferFourOrders(SessionFixture session) : FeatureFixture, IScenarioSetUp, IClassFixture<SessionFixture>
{
    State<MyOrderDto> order1;
    State<MyOrderDto> order2;
    State<MyOrderDto> order3;
    State<MyOrderDto> order4;
    State<MyOrderDto> cancelledOrder1;

    MyOrderDto Order1 => order1.GetValue(nameof(Order1));
    MyOrderDto Order2 => order2.GetValue(nameof(Order2));
    MyOrderDto Order3 => order3.GetValue(nameof(Order3));
    MyOrderDto Order4 => order4.GetValue(nameof(Order4));
    MyOrderDto CancelledOrder1 => cancelledOrder1.GetValue(nameof(CancelledOrder1));

    async Task IScenarioSetUp.OnScenarioSetUp() => await session.UpdateLockBoxCodes();

    async Task GivenAResidentHasPlacedAndPaidAnOrderWithAMediumPrice()
    {
        await session.SignUpAndSignIn();
        // 3x200 + 200 + 500 = 1300
        var getMyOrderResponse = await session.PlaceAndPayResidentOrder(new TestReservation(SeedData.Frederik.ResourceId, 3, PriceGroup.Low));
        order1 = getMyOrderResponse.Order;
        await session.ConfirmOrders();
    }

    async Task GivenAResidentHasPlacedAndPaidAnOrderWithAHighPrice()
    {
        // 1x500 + 500 + 1000 = 2000
        await session.SignUpAndSignIn();
        var getMyOrderResponse = await session.PlaceAndPayResidentOrder(new TestReservation(SeedData.BanquetFacilities.ResourceId, 1, PriceGroup.Low));
        order1 = getMyOrderResponse.Order;
        await session.ConfirmOrders();
    }

    async Task GivenTheResidentHasPlacedButNotPaidAnotherOrder()
    {
        // 200 + 200 + 500 = 900
        var getMyOrderResponse = await ResidentOrderExtensions.PlaceResidentOrder(session, new TestReservation(SeedData.Frederik.ResourceId, 1, PriceGroup.Low));
        order2 = getMyOrderResponse.Order;
    }

    async Task GivenTheResidentHasPlacedButNotPaidAThirdOrder()
    {
        // 200 + 200 + 500 = 900
        var getMyOrderResponse = await ResidentOrderExtensions.PlaceResidentOrder(session, new TestReservation(SeedData.Frederik.ResourceId, 1, PriceGroup.Low));
        order3 = getMyOrderResponse.Order;
    }

    async Task GivenTheResidentHasPlacedButNotPaidAFourthOrder()
    {
        // 200 + 200 + 500 = 900
        var getMyOrderResponse = await ResidentOrderExtensions.PlaceResidentOrder(session, new TestReservation(SeedData.Kaj.ResourceId, 1, PriceGroup.Low));
        order4 = getMyOrderResponse.Order;
    }

    async Task WhenTheResidentCancelsTheFirstOrder()
    {
        var updateMyOrderResponse = await session.CancelResidentReservations(Order1.OrderId, 0);
        cancelledOrder1 = updateMyOrderResponse.Order;
        await session.ConfirmOrders();
    }

    async Task ThenTheFirstOrderIsCancelled()
    {
        var getOrderResponse = await session.GetOrder(Order1.OrderId);
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

    async Task ThenTheThirdOrderIsNotConfirmed()
    {
        var getOrderResponse = await session.GetOrder(Order3.OrderId);
        getOrderResponse.Order.IsHistoryOrder.Should().BeFalse();
        getOrderResponse.Order.Reservations.Single().Status.Should().Be(ReservationStatus.Reserved);
        getOrderResponse.Order.Audits.Select(audit => audit.Type).Should().Equal(OrderAuditType.PlaceOrder);
    }

    async Task ThenTheThirdOrderIsConfirmed()
    {
        var getOrderResponse = await session.GetOrder(Order3.OrderId);
        getOrderResponse.Order.IsHistoryOrder.Should().BeFalse();
        getOrderResponse.Order.Reservations.Single().Status.Should().Be(ReservationStatus.Confirmed);
        getOrderResponse.Order.Audits.Select(audit => audit.Type).Should().Equal(OrderAuditType.PlaceOrder, OrderAuditType.ConfirmOrder);
    }

    async Task ThenTheFourthOrderIsNotConfirmed()
    {
        var getOrderResponse = await session.GetOrder(Order4.OrderId);
        getOrderResponse.Order.IsHistoryOrder.Should().BeFalse();
        getOrderResponse.Order.Reservations.Single().Status.Should().Be(ReservationStatus.Reserved);
        getOrderResponse.Order.Audits.Select(audit => audit.Type).Should().Equal(OrderAuditType.PlaceOrder);
    }

    async Task ThenTheResidentsBalanceIsTheRefundMinusTheCancellationFeeMinusThePriceOfTheOtherOrders()
    {
        var getMyTransactionsResponse = await session.GetMyTransactions();
        var balance = getMyTransactionsResponse.Transactions.Select(transaction => transaction.Amount).Sum();
        var price1 = Order1.Price.Total();
        var price2 = Order2.Price.Total();
        var price3 = Order3.Price.Total();
        var price4 = Order4.Price.Total();
        var cancellationFee1 = CancelledOrder1.AdditionalLineItems.Single().Amount;
        balance.Should().Be(price1 + cancellationFee1 - price2 - price3 - price4);
    }
}
