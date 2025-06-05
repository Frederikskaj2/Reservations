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

sealed partial class ResidentOrder(SessionFixture session) : FeatureFixture, IScenarioSetUp, IClassFixture<SessionFixture>
{
    readonly List<Email> emails = [];

    State<MyOrderDto> order;
    State<GetOrderResponse> getOrderResponse;
    State<GetMyTransactionsResponse> getMyTransactionsResponse;
    State<UpdateMyOrderResponse> updateMyOrderResponse;
    State<Amount> cancellationFee;

    MyOrderDto Order => order.GetValue(nameof(Order));
    GetOrderResponse GetOrderResponse => getOrderResponse.GetValue(nameof(GetOrderResponse));
    GetMyTransactionsResponse GetMyTransactionsResponse => getMyTransactionsResponse.GetValue(nameof(GetMyTransactionsResponse));
    UpdateMyOrderResponse UpdateMyOrderResponse => updateMyOrderResponse.GetValue(nameof(UpdateMyOrderResponse));
    Amount CancellationFee => cancellationFee.GetValue(nameof(CancellationFee));

    async Task IScenarioSetUp.OnScenarioSetUp() => await session.UpdateLockBoxCodes();

    async Task GivenAResidentIsSignedIn() => await session.SignUpAndSignIn();

    async Task WhenAnOrderIsPlaced()
    {
        var getMyOrderResponse = await ResidentOrderExtensions.PlaceResidentOrder(session, new TestReservation(SeedData.Frederik.ResourceId));
        order = getMyOrderResponse.Order;
    }

    async Task WhenTheOrderIsPaid(Amount amount) =>
        await session.PayIn(Order.Payment!.PaymentId, amount);

    async Task WhenTheOrderIsCancelled() =>
        updateMyOrderResponse = await session.CancelResidentReservations(Order.OrderId, 0);

    async Task WhenTheJobToConfirmOrdersExecute() =>
        await session.ConfirmOrders();

    async Task ThenTheResidentReceivesAnOrderReceivedEmail()
    {
        emails.AddRange(await session.DequeueEmails());
        var orderReceivedEmail = emails.OrderReceived();
        orderReceivedEmail.Should().NotBeNull();
        orderReceivedEmail.ToEmail.Should().Be(EmailAddress.FromString(session.User!.Email));
        orderReceivedEmail.ToFullName.Should().Be(session.User.FullName);
        var orderReceived = orderReceivedEmail.OrderReceived!;
        orderReceived.Payment.Should().NotBeNull();
        orderReceived.Payment!.Amount.Should().Be(Order.Price.Total());
        orderReceived.Payment.AccountNumber.Should().NotBeEmpty();
    }

    async Task ThenTheAdministratorReceivesANewOrderEmail()
    {
        emails.AddRange(await session.DequeueEmails());
        var newOrderEmail = emails.NewOrder();
        newOrderEmail.Should().NotBeNull();
        newOrderEmail.ToEmail.Should().Be(EmailAddress.FromString(SeedData.AdministratorEmail));
        var newOrder = newOrderEmail.NewOrder!;
        newOrder.OrderId.Should().Be(Order.OrderId);
    }

    async Task ThenTheResidentReceivesAPayInEmail(Amount amount)
    {
        emails.AddRange(await session.DequeueEmails());
        var payInEmail = emails.PayIn();
        payInEmail.Should().NotBeNull();
        payInEmail.ToEmail.Should().Be(EmailAddress.FromString(session.User!.Email));
        payInEmail.ToFullName.Should().Be(session.User.FullName);
        var payIn = payInEmail.PayIn!;
        payIn.Amount.Should().Be(amount);
        if (amount >= Order.Price.Total())
            payIn.Payment.Should().BeNull();
        else
        {
            payIn.Payment.Should().NotBeNull();
            payIn.Payment!.Amount.Should().Be(Order.Price.Total() - amount);
            payIn.Payment.AccountNumber.Should().NotBeEmpty();
        }
    }

    async Task ThenTheResidentReceivesAReservationCancelledEmail(Amount refund, Amount expectedFee)
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
        reservationsCancelled.Fee.Should().Be(expectedFee);
    }

    async Task ThenTheOrderIsConfirmed()
    {
        getOrderResponse = await session.GetOrder(Order.OrderId);
        GetOrderResponse.Should().NotBeNull();
        GetOrderResponse.Order.Should().NotBeNull();
        GetOrderResponse.Order.Type.Should().Be(OrderType.Resident);
        GetOrderResponse.Order.IsHistoryOrder.Should().BeFalse();
        GetOrderResponse.Order.Reservations.Single().Status.Should().Be(ReservationStatus.Confirmed);
        GetOrderResponse.Order.Audits.Select(audit => audit.Type).Should().Equal(OrderAuditType.PlaceOrder, OrderAuditType.ConfirmOrder);
    }

    async Task ThenTheOrderIsNotConfirmed()
    {
        getOrderResponse = await session.GetOrder(Order.OrderId);
        GetOrderResponse.Order.Should().NotBeNull();
        GetOrderResponse.Order.Type.Should().Be(OrderType.Resident);
        GetOrderResponse.Order.IsHistoryOrder.Should().BeFalse();
        GetOrderResponse.Order.Reservations.Single().Status.Should().Be(ReservationStatus.Reserved);
        GetOrderResponse.Order.Audits.Select(audit => audit.Type).Should().Equal(OrderAuditType.PlaceOrder);
    }

    async Task ThenTheResidentsBalanceIs(Amount amount)
    {
        getMyTransactionsResponse = await session.GetMyTransactions();
        var balance = GetMyTransactionsResponse.Transactions.Select(transaction => transaction.Amount).Sum();
        balance.Should().Be(amount);
    }

    Task ThenTheResidentHasNoOutstandingPayment()
    {
        GetMyTransactionsResponse.Payment.Should().BeNull();
        var balance = GetMyTransactionsResponse.Transactions.Select(transaction => transaction.Amount).Sum();
        balance.Should().BeGreaterOrEqualTo(Amount.Zero);
        return Task.CompletedTask;
    }

    Task ThenTheResidentHasAnOutstandingPayment(Amount amount)
    {
        GetMyTransactionsResponse.Payment.Should().NotBeNull();
        GetMyTransactionsResponse.Payment!.Amount.Should().Be(amount);
        GetMyTransactionsResponse.Payment.AccountNumber.Should().NotBeEmpty();
        var balance = GetMyTransactionsResponse.Transactions.Select(transaction => transaction.Amount).Sum();
        balance.Should().Be(-amount);
        return Task.CompletedTask;
    }

    Task ThenTheOrderIsCancelled()
    {
        UpdateMyOrderResponse.IsUserDeleted.Should().BeFalse();
        UpdateMyOrderResponse.Order.Should().NotBeNull();
        UpdateMyOrderResponse.Order.IsHistoryOrder.Should().BeTrue();
        UpdateMyOrderResponse.Order.Reservations.Single().Status.Should().Be(ReservationStatus.Cancelled);
        UpdateMyOrderResponse.Order.Payment.Should().BeNull();
        return Task.CompletedTask;
    }

    Task ThenTheOrderIsAbandoned()
    {
        UpdateMyOrderResponse.IsUserDeleted.Should().BeFalse();
        UpdateMyOrderResponse.Order.Should().NotBeNull();
        UpdateMyOrderResponse.Order.IsHistoryOrder.Should().BeTrue();
        UpdateMyOrderResponse.Order.Reservations.Single().Status.Should().Be(ReservationStatus.Abandoned);
        UpdateMyOrderResponse.Order.Payment.Should().BeNull();
        return Task.CompletedTask;
    }

    Task ThenTheOrderHasACancellationFee()
    {
        var myLineItem = UpdateMyOrderResponse.Order.AdditionalLineItems.Single();
        myLineItem.Type.Should().Be(LineItemType.CancellationFee);
        myLineItem.CancellationFee.Should().NotBeNull();
        myLineItem.CancellationFee!.Reservations.Should().Equal(ReservationIndex.FromInt32(0));
        myLineItem.Damages.Should().BeNull();
        cancellationFee = myLineItem.Amount;
        CancellationFee.Should().BeLessThan(Amount.Zero);
        return Task.CompletedTask;
    }

    Task ThenTheOrderHasNoCancellationFee()
    {
        UpdateMyOrderResponse.Order.AdditionalLineItems.Should().BeEmpty();
        return Task.CompletedTask;
    }

    async Task ThenTheOrderIsConfirmedAndFinished()
    {
        getOrderResponse = await session.GetOrder(Order.OrderId);
        GetOrderResponse.Order.Audits.Select(audit => audit.Type)
            .Should()
            .Equal(OrderAuditType.PlaceOrder, OrderAuditType.ConfirmOrder, OrderAuditType.CancelReservation, OrderAuditType.FinishOrder);
    }

    async Task ThenTheOrderFinishedWithoutConfirmation()
    {
        getOrderResponse = await session.GetOrder(Order.OrderId);
        GetOrderResponse.Order.Audits.Select(audit => audit.Type)
            .Should()
            .Equal(OrderAuditType.PlaceOrder, OrderAuditType.CancelReservation, OrderAuditType.FinishOrder);
    }

    Task ThenTheOrderIsCancelledWhenViewedByAnAdministrator()
    {
        GetOrderResponse.Order.Should().NotBeNull();
        GetOrderResponse.Order.Type.Should().Be(OrderType.Resident);
        GetOrderResponse.Order.IsHistoryOrder.Should().BeTrue();
        GetOrderResponse.Order.Reservations.Single().Status.Should().Be(ReservationStatus.Cancelled);
        return Task.CompletedTask;
    }

    Task ThenTheOrderIsAbandonedWhenViewedByAnAdministrator()
    {
        GetOrderResponse.Order.Should().NotBeNull();
        GetOrderResponse.Order.Type.Should().Be(OrderType.Resident);
        GetOrderResponse.Order.IsHistoryOrder.Should().BeTrue();
        GetOrderResponse.Order.Reservations.Single().Status.Should().Be(ReservationStatus.Abandoned);
        return Task.CompletedTask;
    }

    Task ThenTheOrderHasACancellationFeeWhenViewedByAnAdministrator()
    {
        var lineItem = GetOrderResponse.Order.Resident!.AdditionalLineItems.Single();
        lineItem.Type.Should().Be(LineItemType.CancellationFee);
        lineItem.Amount.Should().Be(CancellationFee);
        lineItem.CancellationFee.Should().NotBeNull();
        lineItem.CancellationFee!.Reservations.Should().Equal(new ReservationIndex(0));
        lineItem.Damages.Should().BeNull();
        return Task.CompletedTask;
    }

    Task ThenTheOrderHasNoCancellationFeeWhenViewedByAnAdministrator()
    {
        GetOrderResponse.Order.Resident!.AdditionalLineItems.Should().BeEmpty();
        return Task.CompletedTask;
    }
}
