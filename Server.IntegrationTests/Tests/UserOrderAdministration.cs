using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Frederikskaj2.Reservations.Server.IntegrationTests.Harness;
using Frederikskaj2.Reservations.Shared.Core;
using Xunit;

namespace Frederikskaj2.Reservations.Server.IntegrationTests.Tests;

public class UserOrderAdministration : IClassFixture<SessionFixture>
{
    public UserOrderAdministration(SessionFixture session) => Session = session;

    SessionFixture Session { get; }

    [Fact]
    public async Task AdministratorCanUpdateUserAccountNumber()
    {
        await Session.SignUpAndSignInAsync();
        var userOrder = await Session.UserPlaceOrderAsync(new TestReservation(TestData.Kaj.ResourceId, 1, true));
        const string newAccountNumber = "1234-12345678";
        await Session.UpdateAccountNumberAsync(Session.UserId(), userOrder.OrderId, newAccountNumber);
        var myUser = await Session.GetMyUserAsync();
        var user = await Session.GetUserAsync(Session.UserId());
        myUser.AccountNumber.Should().Be(newAccountNumber);
        user.Audits.Select(audit => audit.Type).Should().Equal(
            UserAuditType.SignUp, UserAuditType.ConfirmEmail, UserAuditType.SetAccountNumber, UserAuditType.CreateOrder, UserAuditType.SetAccountNumber);
        user.Audits.ElementAt(2).Should().Match<UserAudit>(audit => audit.UserId == Session.UserId());
        user.Audits.ElementAt(4).Should().Match<UserAudit>(audit => audit.UserId == TestData.AdministratorUserId);
    }

    [Fact]
    public async Task AdministratorCanAllowCancellationWithoutFee()
    {
        await Session.SignUpAndSignInAsync();
        var userOrder = await Session.UserPlaceAndPayOrderAsync(new TestReservation(TestData.Kaj.ResourceId, 1, true));
        var myReservation = userOrder.Reservations!.Single();
        var price = userOrder.Price.Total();
        var order = await Session.GetOrderAsync(userOrder.OrderId);
        await Session.AllowUserToCancelWithoutFee(Session.UserId(), order.OrderId);
        var myOrder = await Session.GetMyOrderAsync(userOrder.OrderId);
        var reservation = myOrder.Reservations!.First();
        var result = await Session.CancelUserReservationNoFeeAsync(userOrder.OrderId, 0);
        var myTransactions = await Session.GetMyTransactionsAsync();
        var myBalance = myTransactions.Transactions.Select(transaction => transaction.Amount).Sum();
        var reservedDays = await Session.GetUserReservedDays();
        var myReservedDays = reservedDays.Where(reservedDay => reservedDay.IsMyReservation);
        myReservation.CanBeCancelled.Should().BeFalse();
        reservation.CanBeCancelled.Should().BeTrue();
        result.Order.Should().NotBeNull();
        result.Order!.IsHistoryOrder.Should().BeTrue();
        result.Order!.Reservations!.First().Status.Should().Be(ReservationStatus.Cancelled);
        myBalance.Should().Be(price);
        myReservedDays.Should().BeEmpty();
    }

    [Fact]
    public async Task AdministratorCanCancelReservation()
    {
        await Session.SignUpAndSignInAsync();
        var userOrder = await Session.UserPlaceOrderAsync(new TestReservation(TestData.Kaj.ResourceId, 1, true));
        var order = await Session.GetOrderAsync(userOrder.OrderId);
        var reservation = order.Reservations.First();
        var cancelledOrder = await Session.CancelReservationAsync(Session.UserId(), userOrder.OrderId, 0);
        var myTransactions = await Session.GetMyTransactionsAsync();
        var myBalance = myTransactions.Transactions.Select(transaction => transaction.Amount).Sum();
        var reservedDays = await Session.GetUserReservedDays();
        var myReservedDays = reservedDays.Where(reservedDay => reservedDay.IsMyReservation);
        var emails = await Session.DequeueEmailsAsync();
        var reservationsCancelledEmail = emails.ReservationsCancelled();
        reservation.CanBeCancelled.Should().BeTrue();
        cancelledOrder.Should().NotBeNull();
        cancelledOrder.Type.Should().Be(OrderType.User);
        cancelledOrder.IsHistoryOrder.Should().BeTrue();
        myBalance.Should().Be(Amount.Zero);
        cancelledOrder.Reservations.Single().Status.Should().Be(ReservationStatus.Abandoned);
        myReservedDays.Should().BeEmpty();
        cancelledOrder.User!.AccountNumber.Should().BeNullOrEmpty();
        myBalance.Should().Be(Amount.Zero);
        myReservedDays.Should().BeEmpty();
        cancelledOrder.Audits.Select(audit => audit.Type).Should().Equal(
            OrderAuditType.PlaceOrder, OrderAuditType.CancelReservation, OrderAuditType.FinishOrder);
        emails.Should().HaveCount(3);
        reservationsCancelledEmail.Should().NotBeNull();
        reservationsCancelledEmail!.Email.Should().Be(order.UserInformation.Email);
        reservationsCancelledEmail.FullName.Should().Be(order.UserInformation.FullName);
        reservationsCancelledEmail.OrderId.Should().Be(order.OrderId);
        reservationsCancelledEmail.Reservations.Should().HaveCount(1);
        reservationsCancelledEmail.Refund.Should().Be(Amount.Zero);
        reservationsCancelledEmail.Fee.Should().Be(Amount.Zero);
    }
}
