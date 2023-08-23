using FluentAssertions;
using Frederikskaj2.Reservations.Server.IntegrationTests.Harness;
using Frederikskaj2.Reservations.Shared.Core;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Frederikskaj2.Reservations.Server.IntegrationTests.Tests;

public class SettleReservation : IClassFixture<SessionFixture>
{
    public SettleReservation(SessionFixture session) => Session = session;

    SessionFixture Session { get; }

    [Fact]
    public async Task PlaceOrderThenSettle()
    {
        await Session.SignUpAndSignInAsync();
        var userOrder = await Session.UserPlaceAndPayOrderAsync(new TestReservation(TestData.Frederik.ResourceId));
        var reservation = userOrder.Reservations!.Single();
        await Session.SettleReservationAsync(Session.UserId(), userOrder.OrderId, 0);
        var order = await Session.GetOrderAsync(userOrder.OrderId);
        var myTransactions = await Session.GetMyTransactionsAsync();
        var myBalance = myTransactions.Transactions.Select(transaction => transaction.Amount).Sum();
        var creditors = await Session.GetCreditorsAsync();
        var creditor = creditors.SingleOrDefault(c => c.UserInformation.UserId == Session.UserId());
        order.Should().NotBeNull();
        order.Type.Should().Be(OrderType.User);
        order.IsHistoryOrder.Should().BeTrue();
        myBalance.Should().Be(reservation.Price!.Deposit);
        order.Reservations.Single().Status.Should().Be(ReservationStatus.Settled);
        order.Audits.Select(audit => audit.Type).Should().Equal(
            OrderAuditType.PlaceOrder, OrderAuditType.ConfirmOrder, OrderAuditType.SettleReservation, OrderAuditType.FinishOrder);
        creditor.Should().NotBeNull();
        creditor!.AccountNumber.Should().NotBeEmpty();
        creditor.Amount.Should().Be(userOrder.Price.Deposit);
    }

    [Fact]
    public async Task PlaceTwoOrdersThenPayAndSettleOne()
    {
        await Session.SignUpAndSignInAsync();
        // Place and pay order 1
        var userOrder1 = await Session.UserPlaceAndPayOrderAsync(new TestReservation(TestData.BanquetFacilities.ResourceId, 2));
        // Place order 2
        var userOrder2 = await Session.UserPlaceOrderAsync(new TestReservation(TestData.Frederik.ResourceId, 1, PriceGroup.Low));
        var price2 = userOrder2.Price.Total();
        // Settle order 1
        var reservation = userOrder1.Reservations!.Single();
        await Session.SettleReservationAsync(Session.UserId(), userOrder1.OrderId, 0);
        var order1 = await Session.GetOrderAsync(userOrder1.OrderId);
        var order2 = await Session.GetOrderAsync(userOrder2.OrderId);
        var order1Deposit = reservation.Price!.Deposit;
        order1Deposit.Should().BeGreaterThan(price2);
        order1.IsHistoryOrder.Should().BeTrue();
        order1.Reservations.Single().Status.Should().Be(ReservationStatus.Settled);
        order2.IsHistoryOrder.Should().BeFalse();
        order2.Reservations.Single().Status.Should().Be(ReservationStatus.Confirmed);
        var myTransactions = await Session.GetMyTransactionsAsync();
        var myBalance = myTransactions.Transactions.Select(transaction => transaction.Amount).Sum();
        myBalance.Should().Be(order1Deposit - price2);
        order1.Audits.Select(audit => audit.Type).Should().Equal(
            OrderAuditType.PlaceOrder, OrderAuditType.ConfirmOrder, OrderAuditType.SettleReservation, OrderAuditType.FinishOrder);
        order2.Audits.Select(audit => audit.Type).Should().Equal(OrderAuditType.PlaceOrder, OrderAuditType.ConfirmOrder);
    }

    [Fact]
    public async Task PlaceOrderThenSettleWithDamages()
    {
        await Session.SignUpAndSignInAsync();
        var userOrder = await Session.UserPlaceAndPayOrderAsync(new TestReservation(TestData.Frederik.ResourceId));
        var remainingAmount = Amount.FromInt32(100);
        var damages = userOrder.Price.Deposit - remainingAmount;
        const string description = "Description of damages";
        var reservationIndex = ReservationIndex.FromInt32(0);
        await Session.SettleReservationAsync(Session.UserId(), userOrder.OrderId, reservationIndex, damages, description);
        var order = await Session.GetOrderAsync(userOrder.OrderId);
        var damagesLineItem = order.User!.AdditionalLineItems.Single();
        var myTransactions = await Session.GetMyTransactionsAsync();
        var myBalance = myTransactions.Transactions.Select(transaction => transaction.Amount).Sum();
        var creditors = await Session.GetCreditorsAsync();
        var creditor = creditors.SingleOrDefault(c => c.UserInformation.UserId == Session.UserId());
        order.Should().NotBeNull();
        order.Type.Should().Be(OrderType.User);
        order.IsHistoryOrder.Should().BeTrue();
        myBalance.Should().Be(remainingAmount);
        order.Reservations.Single().Status.Should().Be(ReservationStatus.Settled);
        damagesLineItem.Type.Should().Be(LineItemType.Damages);
        damagesLineItem.Amount.Should().Be(-damages);
        damagesLineItem.Damages!.Reservation.Should().Be(reservationIndex);
        damagesLineItem.Damages.Description.Should().Be(description);
        order.Audits.Select(audit => audit.Type).Should().Equal(
            OrderAuditType.PlaceOrder, OrderAuditType.ConfirmOrder, OrderAuditType.SettleReservation, OrderAuditType.FinishOrder);
        creditor.Should().NotBeNull();
        creditor!.AccountNumber.Should().NotBeEmpty();
        creditor.Amount.Should().Be(remainingAmount);
    }
}
