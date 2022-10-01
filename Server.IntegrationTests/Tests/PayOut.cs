using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Frederikskaj2.Reservations.Server.IntegrationTests.Harness;
using Frederikskaj2.Reservations.Shared.Core;
using Xunit;

namespace Frederikskaj2.Reservations.Server.IntegrationTests.Tests;

public class PayOut : IClassFixture<SessionFixture>
{
    public PayOut(SessionFixture session) => Session = session;

    SessionFixture Session { get; }

    [Fact]
    public async Task PayOutSettledOrder()
    {
        await Session.SignUpAndSignInAsync();
        var userOrder = await Session.UserPlaceAndPayOrderAsync(new TestReservation(TestData.Frederik.ResourceId));
        var deposit = userOrder.Reservations!.Single().Price!.Deposit;
        await Session.SettleReservationAsync(Session.UserId(), userOrder.OrderId, 0);
        var creditors = await Session.GetCreditorsAsync();
        var creditor = creditors.Single(c => c.UserInformation.UserId == Session.UserId());
        var paidCreditor = await Session.PayOutAsync(creditor.UserInformation.UserId, creditor.Amount);
        var order = await Session.GetOrderAsync(userOrder.OrderId);
        var userTransactions = await Session.GetUserTransactionsAsync();
        var userBalance = userTransactions.Transactions.Select(transaction => transaction.Amount).Sum();
        creditor.AccountNumber.Should().NotBeEmpty();
        creditor.Amount.Should().Be(deposit);
        paidCreditor.AccountNumber.Should().BeNull();
        paidCreditor.Amount.Should().Be(Amount.Zero);
        order.Should().NotBeNull();
        order.Type.Should().Be(OrderType.User);
        order.IsHistoryOrder.Should().BeTrue();
        order.Reservations.Single().Status.Should().Be(ReservationStatus.Settled);
        userBalance.Should().Be(Amount.Zero);
        order.Audits.Select(audit => audit.Type).Should().Equal(
            OrderAuditType.PlaceOrder, OrderAuditType.ConfirmOrder, OrderAuditType.SettleReservation, OrderAuditType.FinishOrder);
    }

    [Fact]
    public async Task PayOutSettledOrderPartially()
    {
        await Session.SignUpAndSignInAsync();
        var userOrder = await Session.UserPlaceAndPayOrderAsync(new TestReservation(TestData.Frederik.ResourceId));
        var deposit = userOrder.Reservations!.Single().Price!.Deposit;
        await Session.SettleReservationAsync(Session.UserId(), userOrder.OrderId, 0);
        var creditors = await Session.GetCreditorsAsync();
        var creditor = creditors.Single(d => d.UserInformation.UserId == Session.UserId());
        var missingAmount = Amount.FromInt32(100);
        var payOut = creditor.Amount - missingAmount;
        var paidCreditor = await Session.PayOutAsync(creditor.UserInformation.UserId, payOut);
        var order = await Session.GetOrderAsync(userOrder.OrderId);
        var userTransactions = await Session.GetUserTransactionsAsync();
        var userBalance = userTransactions.Transactions.Select(transaction => transaction.Amount).Sum();
        creditor.AccountNumber.Should().NotBeEmpty();
        creditor.Amount.Should().Be(deposit);
        paidCreditor.AccountNumber.Should().NotBeEmpty();
        paidCreditor.Amount.Should().Be(missingAmount);
        order.Should().NotBeNull();
        order.Type.Should().Be(OrderType.User);
        order.IsHistoryOrder.Should().BeTrue();
        order.Reservations.Single().Status.Should().Be(ReservationStatus.Settled);
        userBalance.Should().Be(missingAmount);
    }

    [Fact]
    public async Task PayOutMultipleSettledOrders()
    {
        await Session.SignUpAndSignInAsync();
        var userOrder1 = await Session.UserPlaceAndPayOrderAsync(new TestReservation(TestData.Frederik.ResourceId));
        var deposit1 = userOrder1.Reservations!.Single().Price!.Deposit;
        await Session.SettleReservationAsync(Session.UserId(), userOrder1.OrderId, 0);
        var userOrder2 = await Session.UserPlaceAndPayOrderAsync(new TestReservation(TestData.BanquetFacilities.ResourceId, 2));
        var deposit2 = userOrder2.Reservations!.Single().Price!.Deposit;
        await Session.SettleReservationAsync(Session.UserId(), userOrder2.OrderId, 0);
        var creditors = await Session.GetCreditorsAsync();
        var creditor = creditors.Single(d => d.UserInformation.UserId == Session.UserId());
        var paidCreditor = await Session.PayOutAsync(creditor.UserInformation.UserId, creditor.Amount);
        var order1 = await Session.GetOrderAsync(userOrder1.OrderId);
        var order2 = await Session.GetOrderAsync(userOrder2.OrderId);
        var userTransactions = await Session.GetUserTransactionsAsync();
        var userBalance = userTransactions.Transactions.Select(transaction => transaction.Amount).Sum();
        creditor.AccountNumber.Should().NotBeEmpty();
        creditor.Amount.Should().Be(deposit1 + deposit2);
        paidCreditor.AccountNumber.Should().BeNull();
        paidCreditor.Amount.Should().Be(Amount.Zero);
        order1.Should().NotBeNull();
        order1.Type.Should().Be(OrderType.User);
        order1.IsHistoryOrder.Should().BeTrue();
        order1.Reservations.Single().Status.Should().Be(ReservationStatus.Settled);
        order2.Should().NotBeNull();
        order2.Type.Should().Be(OrderType.User);
        order2.IsHistoryOrder.Should().BeTrue();
        order2.Reservations.Single().Status.Should().Be(ReservationStatus.Settled);
        userBalance.Should().Be(Amount.Zero);
    }
}
