using FluentAssertions;
using Frederikskaj2.Reservations.Emails;
using Frederikskaj2.Reservations.Orders;
using Frederikskaj2.Reservations.Server.IntegrationTests.Harness;
using Frederikskaj2.Reservations.Users;
using LightBDD.Core.Extensibility.Execution;
using LightBDD.Framework;
using LightBDD.XUnit2;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Frederikskaj2.Reservations.Server.IntegrationTests.Tests.Orders;

sealed partial class ResidentOrderMultipleReservations(SessionFixture session) : FeatureFixture, IScenarioSetUp, IClassFixture<SessionFixture>
{
    readonly List<Email> emails = [];

    State<MyOrderDto> order;
    State<UpdateMyOrderResponse> updateMyOrderResponse;
    State<Amount> cancellationFee;

    MyOrderDto Order => order.GetValue(nameof(Order));
    UpdateMyOrderResponse UpdateMyOrderResponse => updateMyOrderResponse.GetValue(nameof(UpdateMyOrderResponse));
    Amount CancellationFee => cancellationFee.GetValue(nameof(CancellationFee));

    async Task IScenarioSetUp.OnScenarioSetUp() => await session.UpdateLockBoxCodes();

    async Task GivenAResidentIsSignedIn() => await session.SignUpAndSignIn();

    async Task GivenAnOrderIsPlaced()
    {
        var placeMyOrderResponse = await ResidentOrderExtensions.PlaceResidentOrder(session, new TestReservation(SeedData.Frederik.ResourceId, 2),
            new TestReservation(SeedData.Kaj.ResourceId));
        order = placeMyOrderResponse.Order;
    }

    async Task GivenTheOrderIsPaid()
    {
        await session.PayIn(Order.Payment!.PaymentId, Order.Price.Total());
        await session.ConfirmOrders();
    }

    async Task WhenAReservationsIsCancelled() => updateMyOrderResponse = await session.CancelResidentReservations(Order.OrderId, 0);

    async Task ThenTheResidentReceivesAReservationCancelledEmail(Amount refund, Amount fee)
    {
        emails.AddRange(await session.DequeueEmails());
        var reservationsCancelledEmail = emails.ReservationsCancelled();
        reservationsCancelledEmail.Should().NotBeNull();
        reservationsCancelledEmail.ToEmail.Should().Be(EmailAddress.FromString(session.User!.Email));
        reservationsCancelledEmail.ToFullName.Should().Be(session.User.FullName);
        var reservationsCancelled = reservationsCancelledEmail.ReservationsCancelled!;
        reservationsCancelled.OrderId.Should().Be(Order.OrderId);
        reservationsCancelled.Reservations.Should().ContainSingle();
        reservationsCancelled.Refund.Should().Be(refund);
        reservationsCancelled.Fee.Should().Be(fee);
    }

    Task ThenOneReservationIsAbandonedAndTheOtherReserved()
    {
        UpdateMyOrderResponse.IsUserDeleted.Should().BeFalse();
        UpdateMyOrderResponse.Order.Should().NotBeNull();
        UpdateMyOrderResponse.Order.IsHistoryOrder.Should().BeFalse();
        UpdateMyOrderResponse.Order.Reservations.First().Status.Should().Be(ReservationStatus.Abandoned);
        UpdateMyOrderResponse.Order.Reservations.Skip(1).Single().Status.Should().Be(ReservationStatus.Reserved);
        return Task.CompletedTask;
    }

    Task ThenOneReservationIsCancelledAndTheOtherConfirmed()
    {
        UpdateMyOrderResponse.IsUserDeleted.Should().BeFalse();
        UpdateMyOrderResponse.Order.Should().NotBeNull();
        UpdateMyOrderResponse.Order.IsHistoryOrder.Should().BeFalse();
        UpdateMyOrderResponse.Order.Reservations.First().Status.Should().Be(ReservationStatus.Cancelled);
        UpdateMyOrderResponse.Order.Reservations.Skip(1).Single().Status.Should().Be(ReservationStatus.Confirmed);
        return Task.CompletedTask;
    }

    Task ThenTheOrderHasNoCancellationFee()
    {
        UpdateMyOrderResponse.Order.AdditionalLineItems.Should().BeEmpty();
        return Task.CompletedTask;
    }

    Task ThenTheOrderHasACancellationFee()
    {
        var myLineItem = UpdateMyOrderResponse.Order.AdditionalLineItems.Single();
        myLineItem.Should().NotBeNull();
        myLineItem.Type.Should().Be(LineItemType.CancellationFee);
        myLineItem.CancellationFee.Should().NotBeNull();
        myLineItem.CancellationFee!.Reservations.Should().Equal(ReservationIndex.FromInt32(0));
        myLineItem.Damages.Should().BeNull();
        cancellationFee = myLineItem.Amount;
        CancellationFee.Should().BeLessThan(Amount.Zero);
        return Task.CompletedTask;
    }

    async Task ThenOneReservationIsAbandonedAndTheOtherReservedWhenViewedByAnAdministrator()
    {
        var getOrderResponse = await session.GetOrder(Order.OrderId);
        getOrderResponse.Order.Should().NotBeNull();
        getOrderResponse.Order.Type.Should().Be(OrderType.Resident);
        getOrderResponse.Order.IsHistoryOrder.Should().BeFalse();
        getOrderResponse.Order.Reservations.First().Status.Should().Be(ReservationStatus.Abandoned);
        getOrderResponse.Order.Reservations.Skip(1).Single().Status.Should().Be(ReservationStatus.Reserved);
        getOrderResponse.Order.Audits.Select(audit => audit.Type)
            .Should()
            .Equal(OrderAuditType.PlaceOrder, OrderAuditType.CancelReservation);
    }

    async Task ThenOneReservationIsCancelledAndTheOtherConfirmedWhenViewedByAnAdministrator()
    {
        var getOrderResponse = await session.GetOrder(Order.OrderId);
        getOrderResponse.Order.Should().NotBeNull();
        getOrderResponse.Order.Type.Should().Be(OrderType.Resident);
        getOrderResponse.Order.IsHistoryOrder.Should().BeFalse();
        getOrderResponse.Order.Reservations.First().Status.Should().Be(ReservationStatus.Cancelled);
        getOrderResponse.Order.Reservations.Skip(1).Single().Status.Should().Be(ReservationStatus.Confirmed);
        getOrderResponse.Order.Audits.Select(audit => audit.Type)
            .Should()
            .Equal(OrderAuditType.PlaceOrder, OrderAuditType.ConfirmOrder, OrderAuditType.CancelReservation);
    }

    async Task ThenTheResidentsBalanceIsThePriceOfTheOtherReservation()
    {
        var getMyTransactionsResponse = await session.GetMyTransactions();
        var balance = getMyTransactionsResponse.Transactions.Select(transaction => transaction.Amount).Sum();
        balance.Should().Be(-Order.Reservations.Last().Price!.Total());
    }

    async Task ThenTheResidentsBalanceIsThePriceOfTheReservationMinusTheCancellationFee()
    {
        var getMyTransactionsResponse = await session.GetMyTransactions();
        var balance = getMyTransactionsResponse.Transactions.Select(transaction => transaction.Amount).Sum();
        var orderPrice = Order.Price.Total();
        var reservationPrice = Order.Reservations.First().Price!.Total();
        var remainingPrice = orderPrice - (reservationPrice + CancellationFee);
        var payout = orderPrice - remainingPrice;
        payout.Should().BeGreaterThan(Amount.Zero);
        balance.Should().Be(payout);
    }
}
