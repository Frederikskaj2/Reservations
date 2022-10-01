using FluentAssertions;
using Frederikskaj2.Reservations.Server.IntegrationTests.Harness;
using Frederikskaj2.Reservations.Shared.Core;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Frederikskaj2.Reservations.Server.IntegrationTests.Tests;

public class UserOrder : IClassFixture<SessionFixture>
{
    public UserOrder(SessionFixture session) => Session = session;

    SessionFixture Session { get; }

    [Fact]
    public async Task PlaceOrderThenPayOrder()
    {
        await Session.SignUpAndSignInAsync();
        var userOrder = await Session.UserPlaceAndPayOrderAsync(new TestReservation(TestData.Frederik.ResourceId));
        var order = await Session.GetOrderAsync(userOrder.OrderId);
        var userTransactions = await Session.GetUserTransactionsAsync();
        var userBalance = userTransactions.Transactions.Select(transaction => transaction.Amount).Sum();
        order.Should().NotBeNull();
        order.Type.Should().Be(OrderType.User);
        order.IsHistoryOrder.Should().BeFalse();
        userBalance.Should().Be(Amount.Zero);
        userTransactions.Payment.Should().BeNull();
        order.Reservations.Single().Status.Should().Be(ReservationStatus.Confirmed);
        order.Audits.Select(audit => audit.Type).Should().Equal(OrderAuditType.PlaceOrder, OrderAuditType.ConfirmOrder);
    }

    [Fact]
    public async Task PlaceOrderThenPayOrderThenUpdateAccountNumber()
    {
        await Session.SignUpAndSignInAsync();
        var userOrder = await Session.UserPlaceAndPayOrderAsync(new TestReservation(TestData.Frederik.ResourceId));
        const string accountNumber = "9876-123456789";
        await Session.UserUpdateAccountNumberAsync(userOrder.OrderId, accountNumber);
        var order = await Session.GetOrderAsync(userOrder.OrderId);
        var user = await Session.GetUserAsync(userOrder.UserInformation.UserId);
        order.Should().NotBeNull();
        order.Type.Should().Be(OrderType.User);
        order.IsHistoryOrder.Should().BeFalse();
        order.User!.AccountNumber.Should().Be(accountNumber);
        order.Audits.Select(audit => audit.Type).Should().Equal(OrderAuditType.PlaceOrder, OrderAuditType.ConfirmOrder);
        user.Should().NotBeNull();
        user.Audits.Select(audit => audit.Type).Should().Equal(
            UserAuditType.SignUp, UserAuditType.ConfirmEmail, UserAuditType.SetAccountNumber, UserAuditType.CreateOrder, UserAuditType.PayIn,
            UserAuditType.SetAccountNumber);
    }
    [Fact]
    public async Task PlaceOrderThenPayOrderThenCancelOrder()
    {
        await Session.SignUpAndSignInAsync();
        var userOrder = await Session.UserPlaceAndPayOrderAsync(new TestReservation(TestData.Frederik.ResourceId));
        var price = userOrder.Price.Total();
        var myOrder = await Session.UserCancelReservationsAsync(userOrder.OrderId, 0);
        var myLineItem = myOrder.Order?.AdditionalLineItems.Single();
        var order = await Session.GetOrderAsync(userOrder.OrderId);
        var userTransactions = await Session.GetUserTransactionsAsync();
        var userBalance = userTransactions.Transactions.Select(transaction => transaction.Amount).Sum();
        var lineItem = order.User!.AdditionalLineItems.Single();
        var fee = lineItem.Amount;
        var payout = price + fee;
        myOrder.IsUserDeleted.Should().BeFalse();
        myOrder.Order.Should().NotBeNull();
        myOrder.Order!.IsHistoryOrder.Should().BeTrue();
        myOrder.Order.Reservations!.Single().Status.Should().Be(ReservationStatus.Cancelled);
        myOrder.Order.Payment.Should().BeNull();
        myLineItem.Should().NotBeNull();
        myLineItem!.Type.Should().Be(LineItemType.CancellationFee);
        myLineItem.Amount.Should().Be(fee);
        myLineItem.CancellationFee.Should().NotBeNull();
        myLineItem.CancellationFee!.Reservations.Should().Equal(new ReservationIndex(0));
        myLineItem.Damages.Should().BeNull();
        order.Should().NotBeNull();
        order.Type.Should().Be(OrderType.User);
        order.IsHistoryOrder.Should().BeTrue();
        lineItem.Type.Should().Be(LineItemType.CancellationFee);
        lineItem.Amount.Should().Be(fee);
        lineItem.CancellationFee.Should().NotBeNull();
        lineItem.CancellationFee!.Reservations.Should().Equal(new ReservationIndex(0));
        lineItem.Damages.Should().BeNull();
        fee.Should().BeLessThan(Amount.Zero);
        payout.Should().BeGreaterThan(Amount.Zero);
        userBalance.Should().Be(payout);
        order.Reservations.Single().Status.Should().Be(ReservationStatus.Cancelled);
        order.Audits.Select(audit => audit.Type).Should().Equal(
            OrderAuditType.PlaceOrder, OrderAuditType.ConfirmOrder, OrderAuditType.CancelReservation, OrderAuditType.FinishOrder);
    }

    [Fact]
    public async Task PlaceOrderThenPayOrderThenCancelOneReservation()
    {
        await Session.SignUpAndSignInAsync();
        var userOrder = await Session.UserPlaceAndPayOrderAsync(
            new TestReservation(TestData.Frederik.ResourceId, 2),
            new TestReservation(TestData.Kaj.ResourceId));
        var price = userOrder.Price.Total();
        await Session.UserCancelReservationsAsync(userOrder.OrderId, 0);
        var order = await Session.GetOrderAsync(userOrder.OrderId);
        var userTransactions = await Session.GetUserTransactionsAsync();
        var userBalance = userTransactions.Transactions.Select(transaction => transaction.Amount).Sum();
        var reservationPrice = userOrder.Reservations!.First().Price!.Total();
        var fee = order.User!.AdditionalLineItems.Single().Amount;
        var remainingPrice = price - (reservationPrice + fee);
        var payout = price - remainingPrice;
        order.Should().NotBeNull();
        order.Type.Should().Be(OrderType.User);
        order.IsHistoryOrder.Should().BeFalse();
        fee.Should().BeLessThan(Amount.Zero);
        payout.Should().BeGreaterThan(Amount.Zero);
        userBalance.Should().Be(payout);
        order.Reservations.First().Status.Should().Be(ReservationStatus.Cancelled);
        order.Reservations.Skip(1).Single().Status.Should().Be(ReservationStatus.Confirmed);
        order.Audits.Select(audit => audit.Type).Should().Equal(
            OrderAuditType.PlaceOrder, OrderAuditType.ConfirmOrder, OrderAuditType.CancelReservation);
    }

    [Fact]
    public async Task PlaceOrderThenPayOrderThenCancelDifferentNumberOfReservations()
    {
        await Session.SignUpAndSignInAsync();
        var userOrder = await Session.UserPlaceAndPayOrderAsync(
            new TestReservation(TestData.BanquetFacilities.ResourceId, 2),
            new TestReservation(TestData.Frederik.ResourceId),
            new TestReservation(TestData.Kaj.ResourceId, 2),
            new TestReservation(TestData.Frederik.ResourceId, 3));
        var firstCancellationPrice = userOrder.Reservations!.First().Price!.Total();
        await Session.UserCancelReservationsAsync(userOrder.OrderId, 0);
        var orderAfterFirstCancellation = await Session.GetOrderAsync(userOrder.OrderId);
        var firstCancellationFee = orderAfterFirstCancellation.User!.AdditionalLineItems.Single().Amount;
        var reservationIds = new ReservationIndex[] { 1, 2 };
        var secondCancellationPrice = userOrder.Reservations!.Skip(1).First().Price!.Total() + userOrder.Reservations!.Skip(2).First().Price!.Total();
        await Session.UserCancelReservationsAsync(userOrder.OrderId, reservationIds);
        var orderAfterSecondCancellation = await Session.GetOrderAsync(userOrder.OrderId);
        var secondCancellationFee = orderAfterSecondCancellation.User!.AdditionalLineItems.Skip(1).Single().Amount;
        var userTransactions = await Session.GetUserTransactionsAsync();
        var userBalance = userTransactions.Transactions.Select(transaction => transaction.Amount).Sum();
        firstCancellationFee.Should().BeLessThan(Amount.Zero);
        secondCancellationFee.Should().Be(firstCancellationFee);
        orderAfterSecondCancellation.Audits.Select(audit => audit.Type).Should().Equal(
            OrderAuditType.PlaceOrder, OrderAuditType.ConfirmOrder, OrderAuditType.CancelReservation, OrderAuditType.CancelReservation);
        userBalance.Should().Be(firstCancellationPrice + firstCancellationFee + secondCancellationPrice + secondCancellationFee);
    }

    [Fact]
    public async Task PlaceOrderThenPayTooLittle()
    {
        await Session.SignUpAndSignInAsync();
        var userOrder = await Session.UserPlaceOrderAsync(new TestReservation(TestData.Frederik.ResourceId));
        var price = userOrder.Price.Total();
        var missingAmount = Amount.FromInt32(100);
        var debtor = await Session.GetUserDebtorAsync();
        await Session.PayInAsync(debtor.PaymentId, price - missingAmount);
        var order = await Session.GetOrderAsync(userOrder.OrderId);
        var userTransactions = await Session.GetUserTransactionsAsync();
        var userBalance = userTransactions.Transactions.Select(transaction => transaction.Amount).Sum();
        order.Should().NotBeNull();
        order.Type.Should().Be(OrderType.User);
        order.IsHistoryOrder.Should().BeFalse();
        userBalance.Should().Be(-missingAmount);
        userTransactions.Payment.Should().NotBeNull();
        userTransactions.Payment!.Amount.Should().Be(missingAmount);
        userTransactions.Payment.AccountNumber.Should().NotBeEmpty();
        order.Reservations.Single().Status.Should().Be(ReservationStatus.Reserved);
        order.Audits.Select(audit => audit.Type).Should().Equal(OrderAuditType.PlaceOrder);
    }

    [Fact]
    public async Task PlaceOrderThenPayTooMuch()
    {
        await Session.SignUpAndSignInAsync();
        var userOrder = await Session.UserPlaceOrderAsync(new TestReservation(TestData.Frederik.ResourceId));
        var price = userOrder.Price.Total();
        var excessAmount = Amount.FromInt32(100);
        var debtor = await Session.GetUserDebtorAsync();
        await Session.PayInAsync(debtor.PaymentId, price + excessAmount);
        var order = await Session.GetOrderAsync(userOrder.OrderId);
        var userTransactions = await Session.GetUserTransactionsAsync();
        var userBalance = userTransactions.Transactions.Select(transaction => transaction.Amount).Sum();
        order.Should().NotBeNull();
        order.Type.Should().Be(OrderType.User);
        order.IsHistoryOrder.Should().BeFalse();
        userBalance.Should().Be(excessAmount);
        order.Reservations.Single().Status.Should().Be(ReservationStatus.Confirmed);
        order.Audits.Select(audit => audit.Type).Should().Equal(OrderAuditType.PlaceOrder, OrderAuditType.ConfirmOrder);
    }

    [Fact]
    public async Task PlaceOrderThenPayTwice()
    {
        await Session.SignUpAndSignInAsync();
        var userOrder = await Session.UserPlaceOrderAsync(new TestReservation(TestData.Frederik.ResourceId));
        var price = userOrder.Price.Total();
        var missingAmount = Amount.FromInt32(100);
        var debtor = await Session.GetUserDebtorAsync();
        await Session.PayInAsync(debtor.PaymentId, price - missingAmount);
        await Session.PayInAsync(debtor.PaymentId, missingAmount);
        var order = await Session.GetOrderAsync(userOrder.OrderId);
        var userTransactions = await Session.GetUserTransactionsAsync();
        var userBalance = userTransactions.Transactions.Select(transaction => transaction.Amount).Sum();
        order.Should().NotBeNull();
        order.Type.Should().Be(OrderType.User);
        order.IsHistoryOrder.Should().BeFalse();
        userBalance.Should().Be(Amount.Zero);
        order.Reservations.Single().Status.Should().Be(ReservationStatus.Confirmed);
        order.Audits.Select(audit => audit.Type).Should().Equal(OrderAuditType.PlaceOrder, OrderAuditType.ConfirmOrder);
    }

    [Fact]
    public async Task PlaceOrdersThenPayOneAndCancelBoth()
    {
        // A user makes a reservation and pays. The user then makes another
        // reservation but then decides to cancel both reservations (this is
        // a possible scenario when the user tries to change a reservation
        // by creating a new order).
        await Session.SignUpAndSignInAsync();
        // Place and pay order 1
        var userOrder1 = await Session.UserPlaceAndPayOrderAsync(new TestReservation(TestData.Frederik.ResourceId, 1, PriceGroup.Low));
        var price1 = userOrder1.Price.Total();
        // Place order 2
        var userOrder2 = await Session.UserPlaceOrderAsync(new TestReservation(TestData.Kaj.ResourceId, 1, PriceGroup.Low));
        // Cancel order 1
        var cancelResult1 = await Session.UserCancelReservationsAsync(userOrder1.OrderId, 0);
        var cancelledUserOrder1 = cancelResult1.Order!;
        var fee1 = cancelledUserOrder1.AdditionalLineItems.Single().Amount;
        // Cancel order 2
        var cancelResult2 = await Session.UserCancelReservationsAsync(userOrder2.OrderId, 0);
        var cancelledUserOrder2 = cancelResult2.Order!;
        var order1 = await Session.GetOrderAsync(userOrder1.OrderId);
        var order2 = await Session.GetOrderAsync(userOrder2.OrderId);
        order1.IsHistoryOrder.Should().BeTrue();
        order1.Reservations.Single().Status.Should().Be(ReservationStatus.Cancelled);
        order2.IsHistoryOrder.Should().BeTrue();
        order2.Reservations.Single().Status.Should().Be(ReservationStatus.Abandoned);
        cancelledUserOrder2.AdditionalLineItems.Should().BeEmpty();
        var userTransactions = await Session.GetUserTransactionsAsync();
        var userBalance = userTransactions.Transactions.Select(transaction => transaction.Amount).Sum();
        fee1.Should().BeLessThan(Amount.Zero);
        userBalance.Should().Be(price1 + fee1);
        order1.Audits.Select(audit => audit.Type).Should().Equal(
            OrderAuditType.PlaceOrder, OrderAuditType.ConfirmOrder, OrderAuditType.CancelReservation, OrderAuditType.FinishOrder);
        order2.Audits.Select(audit => audit.Type).Should().Equal(OrderAuditType.PlaceOrder, OrderAuditType.CancelReservation, OrderAuditType.FinishOrder);
    }
}
