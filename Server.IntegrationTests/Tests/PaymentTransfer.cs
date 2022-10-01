using FluentAssertions;
using Frederikskaj2.Reservations.Server.IntegrationTests.Harness;
using Frederikskaj2.Reservations.Shared.Core;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Frederikskaj2.Reservations.Server.IntegrationTests.Tests;

// TODO: Test payment transfer after settlement of first order.
public class PaymentTransfer : IClassFixture<SessionFixture>
{
    public PaymentTransfer(SessionFixture session) => Session = session;

    SessionFixture Session { get; }

    [Fact]
    public async Task PaymentTransferSameOrderWithFee()
    {
        await Session.SignUpAndSignInAsync();
        var userOrder1 = await Session.UserPlaceAndPayOrderAsync(new TestReservation(TestData.Frederik.ResourceId, 1, PriceGroup.Low));
        var cancelResult = await Session.UserCancelReservationsAsync(userOrder1.OrderId, 0);
        var cancelledUserOrder1 = cancelResult.Order!;
        var fee = cancelledUserOrder1.AdditionalLineItems.Single().Amount;
        var userOrder2 = await Session.UserPlaceOrderAsync(new TestReservation(TestData.Frederik.ResourceId, 1, PriceGroup.Low));
        var debtor = await Session.GetUserDebtorAsync();
        await Session.PayInAsync(debtor.PaymentId, -fee);
        var order1 = await Session.GetOrderAsync(userOrder1.OrderId);
        var order2 = await Session.GetOrderAsync(userOrder2.OrderId);
        var userTransactions = await Session.GetUserTransactionsAsync();
        var userBalance = userTransactions.Transactions.Select(transaction => transaction.Amount).Sum();
        debtor.Amount.Should().Be(-fee);
        order1.Should().NotBeNull();
        order1.Type.Should().Be(OrderType.User);
        order1.IsHistoryOrder.Should().BeTrue();
        order1.Reservations.Single().Status.Should().Be(ReservationStatus.Cancelled);
        order2.IsHistoryOrder.Should().BeFalse();
        order2.Reservations.Single().Status.Should().Be(ReservationStatus.Confirmed);
        fee.Should().BeLessThan(Amount.Zero);
        userBalance.Should().Be(Amount.Zero);
        order1.Audits.Select(audit => audit.Type).Should().Equal(
            OrderAuditType.PlaceOrder, OrderAuditType.ConfirmOrder, OrderAuditType.CancelReservation, OrderAuditType.FinishOrder);
        order2.Audits.Select(audit => audit.Type).Should().Equal(OrderAuditType.PlaceOrder, OrderAuditType.ConfirmOrder);
    }

    [Fact]
    public async Task PaymentTransferTwoOrdersToOneOrder()
    {
        await Session.SignUpAndSignInAsync();
        // Place and pay order 1
        var userOrder1 = await Session.UserPlaceAndPayOrderAsync(new TestReservation(TestData.Frederik.ResourceId, 1, PriceGroup.Low));
        var price1 = userOrder1.Price.Total();
        // Place and pay order 2
        var userOrder2 = await Session.UserPlaceAndPayOrderAsync(new TestReservation(TestData.Kaj.ResourceId, 1, PriceGroup.Low));
        var price2 = userOrder2.Price.Total();
        // Place order 3
        var userOrder3 = await Session.UserPlaceOrderAsync(new TestReservation(TestData.Frederik.ResourceId, 1, PriceGroup.Low));
        var price3 = userOrder3.Price.Total();
        // Cancel order 1
        var cancelResult1 = await Session.UserCancelReservationsAsync(userOrder1.OrderId, 0);
        var cancelledUserOrder1 = cancelResult1.Order!;
        var fee1 = cancelledUserOrder1.AdditionalLineItems.Single().Amount;
        // Cancel order 2
        var cancelResult2 = await Session.UserCancelReservationsAsync(userOrder2.OrderId, 0);
        var cancelledUserOrder2 = cancelResult2.Order!;
        var fee2 = cancelledUserOrder2.AdditionalLineItems.Single().Amount;
        var order1 = await Session.GetOrderAsync(userOrder1.OrderId);
        var order2 = await Session.GetOrderAsync(userOrder2.OrderId);
        var order3 = await Session.GetOrderAsync(userOrder3.OrderId);
        var userTransactions = await Session.GetUserTransactionsAsync();
        var userBalance = userTransactions.Transactions.Select(transaction => transaction.Amount).Sum();
        order1.IsHistoryOrder.Should().BeTrue();
        order1.Reservations.Single().Status.Should().Be(ReservationStatus.Cancelled);
        order2.IsHistoryOrder.Should().BeTrue();
        order2.Reservations.Single().Status.Should().Be(ReservationStatus.Cancelled);
        order3.IsHistoryOrder.Should().BeFalse();
        order3.Reservations.Single().Status.Should().Be(ReservationStatus.Confirmed);
        fee1.Should().BeLessThan(Amount.Zero);
        fee2.Should().BeLessThan(Amount.Zero);
        userBalance.Should().Be(price1 + price2 - price3 + fee1 + fee2);
        order1.Audits.Select(audit => audit.Type).Should().Equal(
            OrderAuditType.PlaceOrder, OrderAuditType.ConfirmOrder, OrderAuditType.CancelReservation, OrderAuditType.FinishOrder);
        order2.Audits.Select(audit => audit.Type).Should().Equal(
            OrderAuditType.PlaceOrder, OrderAuditType.ConfirmOrder, OrderAuditType.CancelReservation, OrderAuditType.FinishOrder);
        order3.Audits.Select(audit => audit.Type).Should().Equal(OrderAuditType.PlaceOrder, OrderAuditType.ConfirmOrder);
    }

    [Fact]
    public async Task PaymentTransferOneOrderToTwoOrders()
    {
        await Session.SignUpAndSignInAsync();
        // Place and pay order 1
        var userOrder1 = await Session.UserPlaceAndPayOrderAsync(new TestReservation(TestData.Frederik.ResourceId, 7));
        var price1 = userOrder1.Price.Total();
        // Place order 2
        var userOrder2 = await Session.UserPlaceOrderAsync(new TestReservation(TestData.Kaj.ResourceId, 1, PriceGroup.Low));
        var price2 = userOrder2.Price.Total();
        // Place order 3
        var userOrder3 = await Session.UserPlaceOrderAsync(new TestReservation(TestData.Frederik.ResourceId, 1, PriceGroup.Low));
        var price3 = userOrder3.Price.Total();
        // Cancel order 1
        var cancelResult = await Session.UserCancelReservationsAsync(userOrder1.OrderId, 0);
        var cancelledUserOrder = cancelResult.Order!;
        var fee = cancelledUserOrder.AdditionalLineItems.Single().Amount;
        var order1 = await Session.GetOrderAsync(userOrder1.OrderId);
        var order2 = await Session.GetOrderAsync(userOrder2.OrderId);
        var order3 = await Session.GetOrderAsync(userOrder3.OrderId);
        var userTransactions = await Session.GetUserTransactionsAsync();
        var userBalance = userTransactions.Transactions.Select(transaction => transaction.Amount).Sum();
        order1.IsHistoryOrder.Should().BeTrue();
        order1.Reservations.Single().Status.Should().Be(ReservationStatus.Cancelled);
        order2.IsHistoryOrder.Should().BeFalse();
        order2.Reservations.Single().Status.Should().Be(ReservationStatus.Confirmed);
        order3.IsHistoryOrder.Should().BeFalse();
        order3.Reservations.Single().Status.Should().Be(ReservationStatus.Confirmed);
        fee.Should().BeLessThan(Amount.Zero);
        userBalance.Should().Be(price1 + fee - price2 - price3);
        order1.Audits.Select(audit => audit.Type).Should().Equal(
            OrderAuditType.PlaceOrder, OrderAuditType.ConfirmOrder, OrderAuditType.CancelReservation, OrderAuditType.FinishOrder);
        order2.Audits.Select(audit => audit.Type).Should().Equal(OrderAuditType.PlaceOrder, OrderAuditType.ConfirmOrder);
        order3.Audits.Select(audit => audit.Type).Should().Equal(OrderAuditType.PlaceOrder, OrderAuditType.ConfirmOrder);
    }

    [Fact]
    public async Task PaymentTransferOneOrderToThreeOrdersConfirmingOneOrder()
    {
        await Session.SignUpAndSignInAsync();
        // Place and pay order 1
        // 3x200 + 200 + 500 = 1300
        var userOrder1 = await Session.UserPlaceAndPayOrderAsync(new TestReservation(TestData.Frederik.ResourceId, 3, PriceGroup.Low));
        var price1 = userOrder1.Price.Total();
        // Place order 2
        // 200 + 200 + 500 = 900
        var userOrder2 = await Session.UserPlaceOrderAsync(new TestReservation(TestData.Kaj.ResourceId, 1, PriceGroup.Low));
        var price2 = userOrder2.Price.Total();
        // Place order 3
        // 200 + 200 + 500 = 900
        var userOrder3 = await Session.UserPlaceOrderAsync(new TestReservation(TestData.Frederik.ResourceId, 1, PriceGroup.Low));
        var price3 = userOrder3.Price.Total();
        // Place order 4
        // 200 + 200 + 500 = 900
        var userOrder4 = await Session.UserPlaceOrderAsync(new TestReservation(TestData.Kaj.ResourceId, 1, PriceGroup.Low));
        var price4 = userOrder4.Price.Total();
        // Cancel order 1
        var cancelResult = await Session.UserCancelReservationsAsync(userOrder1.OrderId, 0);
        var cancelledUserOrder = cancelResult.Order!;
        var fee = cancelledUserOrder.AdditionalLineItems.Single().Amount;
        var order1 = await Session.GetOrderAsync(userOrder1.OrderId);
        var order2 = await Session.GetOrderAsync(userOrder2.OrderId);
        var order3 = await Session.GetOrderAsync(userOrder3.OrderId);
        var order4 = await Session.GetOrderAsync(userOrder4.OrderId);
        var userTransactions = await Session.GetUserTransactionsAsync();
        var userBalance = userTransactions.Transactions.Select(transaction => transaction.Amount).Sum();
        order1.IsHistoryOrder.Should().BeTrue();
        order1.Reservations.Single().Status.Should().Be(ReservationStatus.Cancelled);
        order2.IsHistoryOrder.Should().BeFalse();
        order2.Reservations.Single().Status.Should().Be(ReservationStatus.Confirmed);
        order3.IsHistoryOrder.Should().BeFalse();
        order3.Reservations.Single().Status.Should().Be(ReservationStatus.Reserved);
        order4.IsHistoryOrder.Should().BeFalse();
        order4.Reservations.Single().Status.Should().Be(ReservationStatus.Reserved);
        fee.Should().BeLessThan(Amount.Zero);
        userBalance.Should().Be(price1 + fee - price2 - price3 - price4);
        order1.Audits.Select(audit => audit.Type).Should().Equal(
            OrderAuditType.PlaceOrder, OrderAuditType.ConfirmOrder, OrderAuditType.CancelReservation, OrderAuditType.FinishOrder);
        order2.Audits.Select(audit => audit.Type).Should().Equal(OrderAuditType.PlaceOrder, OrderAuditType.ConfirmOrder);
        order3.Audits.Select(audit => audit.Type).Should().Equal(OrderAuditType.PlaceOrder);
        order4.Audits.Select(audit => audit.Type).Should().Equal(OrderAuditType.PlaceOrder);
    }

    [Fact]
    public async Task PaymentTransferOneOrderToThreeOrdersConfirmingTwoOrders()
    {
        await Session.SignUpAndSignInAsync();
        // Place and pay order 1
        // 1x500 + 500 + 1000 = 2000
        var userOrder1 = await Session.UserPlaceAndPayOrderAsync(new TestReservation(TestData.BanquetFacilities.ResourceId, 1, PriceGroup.Low));
        var price1 = userOrder1.Price.Total();
        // Place order 2
        // 200 + 200 + 500 = 900
        var userOrder2 = await Session.UserPlaceOrderAsync(new TestReservation(TestData.Kaj.ResourceId, 1, PriceGroup.Low));
        var price2 = userOrder2.Price.Total();
        // Place order 3
        // 200 + 200 + 500 = 900
        var userOrder3 = await Session.UserPlaceOrderAsync(new TestReservation(TestData.Frederik.ResourceId, 1, PriceGroup.Low));
        var price3 = userOrder3.Price.Total();
        // Place order 4
        // 200 + 200 + 500 = 900
        var userOrder4 = await Session.UserPlaceOrderAsync(new TestReservation(TestData.Kaj.ResourceId, 1, PriceGroup.Low));
        var price4 = userOrder4.Price.Total();
        // Cancel order 1
        var cancelResult = await Session.UserCancelReservationsAsync(userOrder1.OrderId, 0);
        var cancelledUserOrder = cancelResult.Order!;
        var fee = cancelledUserOrder.AdditionalLineItems.Single().Amount;
        var order1 = await Session.GetOrderAsync(userOrder1.OrderId);
        var order2 = await Session.GetOrderAsync(userOrder2.OrderId);
        var order3 = await Session.GetOrderAsync(userOrder3.OrderId);
        var order4 = await Session.GetOrderAsync(userOrder4.OrderId);
        var userTransactions = await Session.GetUserTransactionsAsync();
        var userBalance = userTransactions.Transactions.Select(transaction => transaction.Amount).Sum();
        order1.IsHistoryOrder.Should().BeTrue();
        order1.Reservations.Single().Status.Should().Be(ReservationStatus.Cancelled);
        order2.IsHistoryOrder.Should().BeFalse();
        order2.Reservations.Single().Status.Should().Be(ReservationStatus.Confirmed);
        order3.IsHistoryOrder.Should().BeFalse();
        order3.Reservations.Single().Status.Should().Be(ReservationStatus.Confirmed);
        order4.IsHistoryOrder.Should().BeFalse();
        order4.Reservations.Single().Status.Should().Be(ReservationStatus.Reserved);
        fee.Should().BeLessThan(Amount.Zero);
        userBalance.Should().Be(price1 + fee - price2 - price3 - price4);
        order1.Audits.Select(audit => audit.Type).Should().Equal(
            OrderAuditType.PlaceOrder, OrderAuditType.ConfirmOrder, OrderAuditType.CancelReservation, OrderAuditType.FinishOrder);
        order2.Audits.Select(audit => audit.Type).Should().Equal(OrderAuditType.PlaceOrder, OrderAuditType.ConfirmOrder);
        order3.Audits.Select(audit => audit.Type).Should().Equal(OrderAuditType.PlaceOrder, OrderAuditType.ConfirmOrder);
        order4.Audits.Select(audit => audit.Type).Should().Equal(OrderAuditType.PlaceOrder);
    }

    [Fact]
    public async Task PlaceOneOrderAndPayThenPlaceAnotherOrderAndFullyPayIt()
    {
        // A user makes a reservation and pays. The user then makes another
        // reservation and wants the second reservation to "replace" the
        // first. Additionally, the second order is slightly more expensive.
        await Session.SignUpAndSignInAsync();
        // Place and pay order 1
        var userOrder1 = await Session.UserPlaceAndPayOrderAsync(new TestReservation(TestData.BanquetFacilities.ResourceId, 2, PriceGroup.Low));
        var price1 = userOrder1.Price.Total();
        // Place order 2
        var userOrder2 = await Session.UserPlaceOrderAsync(new TestReservation(TestData.BanquetFacilities.ResourceId, 2, PriceGroup.High));
        var price2 = userOrder2.Price.Total();
        // Allow user to cancel order 1 without fee.
        var userOrder = await Session.GetOrderAsync(userOrder1.OrderId);
        await Session.AllowUserToCancelWithoutFee(Session.UserId(), userOrder.OrderId);
        // Cancel order 1 without fee.
        await Session.CancelUserReservationNoFeeAsync(userOrder1.OrderId, 0);
        var myOrder2 = await Session.GetMyOrderAsync(userOrder2.OrderId);
        // Pay remaining amount on order 2.
        await Session.PayInAsync(myOrder2.Payment!.PaymentId, myOrder2.Payment.Amount);
        var order1 = await Session.GetOrderAsync(userOrder1.OrderId);
        var order2 = await Session.GetOrderAsync(userOrder2.OrderId);
        var userTransactions = await Session.GetUserTransactionsAsync();
        var userBalance = userTransactions.Transactions.Select(transaction => transaction.Amount).Sum();
        order1.IsHistoryOrder.Should().BeTrue();
        order1.Reservations.Single().Status.Should().Be(ReservationStatus.Cancelled);
        order2.IsHistoryOrder.Should().BeFalse();
        order2.Reservations.Single().Status.Should().Be(ReservationStatus.Confirmed);
        myOrder2.Payment.Amount.Should().Be(price2 - price1);
        userBalance.Should().Be(Amount.Zero);
        order1.Audits.Select(audit => audit.Type).Should().Equal(
            OrderAuditType.PlaceOrder, OrderAuditType.ConfirmOrder, OrderAuditType.AllowCancellationWithoutFee, OrderAuditType.CancelReservation,
            OrderAuditType.FinishOrder);
        order2.Audits.Select(audit => audit.Type).Should().Equal(OrderAuditType.PlaceOrder, OrderAuditType.ConfirmOrder);
    }
}
