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

sealed partial class AdministratorResidentOrderUpdate(SessionFixture session) : FeatureFixture, IScenarioSetUp, IClassFixture<SessionFixture>
{
    State<MyOrderDto> myOrder;
    State<OrderDetailsDto> order;

    MyOrderDto MyOrder => myOrder.GetValue(nameof(MyOrder));
    OrderDetailsDto Order => order.GetValue(nameof(Order));

    async Task IScenarioSetUp.OnScenarioSetUp() => await session.UpdateLockBoxCodes();

    async Task GivenAResidentHasPlacedAnOrder()
    {
        await session.SignUpAndSignIn();
        var placeMyOrderResponse = await ResidentOrderExtensions.PlaceResidentOrder(session, new TestReservation(SeedData.Kaj.ResourceId, 1, TestReservationType.Soon));
        myOrder = placeMyOrderResponse.Order;
    }

    async Task WhenTheAdministratorCancelsTheReservation()
    {
        var updateResidentOrderResponse = await session.CancelReservation(MyOrder.OrderId, 0);
        order = updateResidentOrderResponse.Order;
    }

    Task ThenTheOrderIsCancelled()
    {
        Order.Should().NotBeNull();
        Order.Type.Should().Be(OrderType.Resident);
        Order.IsHistoryOrder.Should().BeTrue();
        Order.Reservations.Single().Status.Should().Be(ReservationStatus.Abandoned);
        return Task.CompletedTask;
    }

    Task AndTheCancellationIsAudited()
    {
        Order.Audits.Select(audit => audit.Type).Should().Equal(OrderAuditType.PlaceOrder, OrderAuditType.CancelReservation, OrderAuditType.FinishOrder);
        Order.Audits.ElementAt(1).Should().Match<OrderAuditDto>(audit => audit.UserId == SeedData.AdministratorUserId);
        return Task.CompletedTask;
    }

    async Task AndTheResidentDoesNotHaveAnyReservedDaysInTheCalendar()
    {
        var getReservedDaysResponse = await session.GetMyReservedDays();
        var myReservedDays = getReservedDaysResponse.ReservedDays.Where(reservedDay => reservedDay.IsMyReservation);
        myReservedDays.Should().BeEmpty();
    }

    async Task AndTheBalanceOfTheResidentIsZero()
    {
        var myTransactions = await session.GetMyTransactions();
        var myBalance = myTransactions.Transactions.Select(transaction => transaction.Amount).Sum();
        myBalance.Should().Be(Amount.Zero);
    }

    async Task AndTheResidentIsSentAnEmailAboutTheCancellation()
    {
        var emails = await session.DequeueEmails();
        emails.Should().HaveCount(3);
        var reservationsCancelledEmail = emails.ReservationsCancelled();
        reservationsCancelledEmail.Should().NotBeNull();
        reservationsCancelledEmail.ToEmail.ToString().Should().Be(session.User!.Email);
        reservationsCancelledEmail.ToFullName.Should().Be(session.User.FullName);
        var reservationsCancelled = reservationsCancelledEmail.ReservationsCancelled!;
        reservationsCancelled.OrderId.Should().Be(MyOrder.OrderId);
        reservationsCancelled.Reservations.Should().ContainSingle();
        reservationsCancelled.Refund.Should().Be(Amount.Zero);
        reservationsCancelled.Fee.Should().Be(Amount.Zero);
    }
}
