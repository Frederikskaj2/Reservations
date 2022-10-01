using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Frederikskaj2.Reservations.Server.IntegrationTests.Harness;
using Frederikskaj2.Reservations.Shared.Core;
using Xunit;

namespace Frederikskaj2.Reservations.Server.IntegrationTests.Tests;

public class UnpaidUserOrder : IClassFixture<SessionFixture>
{
    public UnpaidUserOrder(SessionFixture session) => Session = session;

    SessionFixture Session { get; }

    [Fact]
    public async Task PlaceOrder()
    {
        await Session.SignUpAndSignInAsync();
        var userOrder = await Session.UserPlaceOrderAsync(new TestReservation(TestData.Frederik.ResourceId));
        var price = userOrder.Price.Total();
        var order = await Session.GetOrderAsync(userOrder.OrderId);
        var reservation = order.Reservations.Single();
        var userTransactions = await Session.GetUserTransactionsAsync();
        var userBalance = userTransactions.Transactions.Select(transaction => transaction.Amount).Sum();
        var reservedDays = await Session.GetUserReservedDays();
        var myReservedDays = reservedDays.Where(reservedDay => reservedDay.IsMyReservation);
        var emails = await Session.DequeueEmailsAsync();
        var orderReceivedEmail = emails.OrderReceived();
        var newOrderEmail = emails.NewOrder();
        userOrder.Payment.Should().NotBeNull();
        userOrder.Payment!.Amount.Should().Be(price);
        userOrder.Payment.AccountNumber.Should().NotBeEmpty();
        order.Should().NotBeNull();
        order.Type.Should().Be(OrderType.User);
        order.User!.AccountNumber.Should().NotBeNullOrEmpty();
        userBalance.Should().Be(-price);
        reservation.Status.Should().Be(ReservationStatus.Reserved);
        reservedDays.Should().Contain(reservation.ToMyReservedDays(order.OrderId, true));
        myReservedDays.Should().Equal(reservation.ToMyReservedDays(order.OrderId, true));
        order.Audits.Select(audit => audit.Type).Should().Equal(OrderAuditType.PlaceOrder);
        emails.Should().HaveCount(2);
        orderReceivedEmail.Email.Should().Be(order.UserInformation.Email);
        orderReceivedEmail.FullName.Should().Be(order.UserInformation.FullName);
        orderReceivedEmail.Payment.Should().NotBeNull();
        orderReceivedEmail.Payment!.Amount.Should().Be(price);
        orderReceivedEmail.Payment.AccountNumber.Should().NotBeEmpty();
        newOrderEmail.Email.Should().Be(EmailAddress.FromString(TestData.AdministratorEmail));
        newOrderEmail.OrderId.Should().Be(userOrder.OrderId);
    }

    [Fact]
    public async Task PlaceOrderThenCancelOrder()
    {
        await Session.SignUpAndSignInAsync();
        var userOrder = await Session.UserPlaceOrderAsync(new TestReservation(TestData.Frederik.ResourceId));
        await Session.UserCancelReservationsAsync(userOrder.OrderId, 0);
        var order = await Session.GetOrderAsync(userOrder.OrderId);
        var reservation = order.Reservations.Single();
        var userTransactions = await Session.GetUserTransactionsAsync();
        var userBalance = userTransactions.Transactions.Select(transaction => transaction.Amount).Sum();
        var reservedDays = await Session.GetUserReservedDays();
        var myReservedDays = reservedDays.Where(reservedDay => reservedDay.IsMyReservation);
        var emails = await Session.DequeueEmailsAsync();
        var reservationsCancelledEmail = emails.ReservationsCancelled();
        order.Should().NotBeNull();
        order.Type.Should().Be(OrderType.User);
        order.IsHistoryOrder.Should().BeTrue();
        order.User!.AccountNumber.Should().BeNullOrEmpty();
        userBalance.Should().Be(Amount.Zero);
        reservation.Status.Should().Be(ReservationStatus.Abandoned);
        myReservedDays.Should().BeEmpty();
        order.Audits.Select(audit => audit.Type).Should().Equal(OrderAuditType.PlaceOrder, OrderAuditType.CancelReservation, OrderAuditType.FinishOrder);
        emails.Should().HaveCount(3);
        reservationsCancelledEmail.Email.Should().Be(order.UserInformation.Email);
        reservationsCancelledEmail.FullName.Should().Be(order.UserInformation.FullName);
        reservationsCancelledEmail.OrderId.Should().Be(order.OrderId);
        reservationsCancelledEmail.Reservations.Should().HaveCount(1);
        reservationsCancelledEmail.Refund.Should().Be(Amount.Zero);
        reservationsCancelledEmail.Fee.Should().Be(Amount.Zero);
    }

    [Fact]
    public async Task PlaceOrderThenCancelOneReservation()
    {
        await Session.SignUpAndSignInAsync();
        var userOrder = await Session.UserPlaceOrderAsync(
            new TestReservation(TestData.Frederik.ResourceId, 2),
            new TestReservation(TestData.Kaj.ResourceId));
        var result = await Session.UserCancelReservationsAsync(userOrder.OrderId, 0);
        var price = userOrder.Price.Total();
        var order = await Session.GetOrderAsync(userOrder.OrderId);
        var reservation = order.Reservations.Skip(1).Single();
        var userTransactions = await Session.GetUserTransactionsAsync();
        var userBalance = userTransactions.Transactions.Select(transaction => transaction.Amount).Sum();
        var reservationPrice = userOrder.Reservations!.First().Price!.Total();
        var remainingPrice = price - reservationPrice;
        var reservedDays = await Session.GetUserReservedDays();
        var myReservedDays = reservedDays.Where(reservedDay => reservedDay.IsMyReservation);
        var emails = await Session.DequeueEmailsAsync();
        var reservationsCancelledEmail = emails.ReservationsCancelled();
        result.Order.Should().NotBeNull();
        result.Order!.Payment.Should().NotBeNull();
        result.Order.Payment!.Amount.Should().Be(remainingPrice);
        result.Order.Payment.AccountNumber.Should().NotBeEmpty();
        order.Should().NotBeNull();
        order.Type.Should().Be(OrderType.User);
        order.IsHistoryOrder.Should().BeFalse();
        userBalance.Should().Be(-remainingPrice);
        order.Reservations.First().Status.Should().Be(ReservationStatus.Abandoned);
        order.Reservations.Skip(1).Single().Status.Should().Be(ReservationStatus.Reserved);
        reservedDays.Should().Contain(reservation.ToMyReservedDays(order.OrderId, true));
        myReservedDays.Should().Equal(reservation.ToMyReservedDays(order.OrderId, true));
        order.Audits.Select(audit => audit.Type).Should().Equal(OrderAuditType.PlaceOrder, OrderAuditType.CancelReservation);
        emails.Should().HaveCount(3);
        reservationsCancelledEmail.Email.Should().Be(order.UserInformation.Email);
        reservationsCancelledEmail.FullName.Should().Be(order.UserInformation.FullName);
        reservationsCancelledEmail.OrderId.Should().Be(order.OrderId);
        reservationsCancelledEmail.Reservations.Should().HaveCount(1);
        reservationsCancelledEmail.Refund.Should().Be(Amount.Zero);
        reservationsCancelledEmail.Fee.Should().Be(Amount.Zero);
    }
}
