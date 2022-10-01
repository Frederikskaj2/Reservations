using FluentAssertions;
using Frederikskaj2.Reservations.Server.IntegrationTests.Harness;
using Frederikskaj2.Reservations.Shared.Core;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Frederikskaj2.Reservations.Server.IntegrationTests.Tests;

public class OwnerOrder : IClassFixture<SessionFixture>
{
    public OwnerOrder(SessionFixture session) => Session = session;

    SessionFixture Session { get; }

    [Fact]
    public async Task PlaceOrder()
    {
        var order = await Session.PlaceOwnerOrderAsync(new TestReservation(TestData.Frederik.ResourceId));
        var reservation = order.Reservations.Single();
        var ownerOrders = await Session.GetOwnerOrdersAsync();
        var reservedDays = await Session.GetOwnerReservedDays();
        var ownerReservedDays = reservedDays.Where(reservedDay => reservedDay.IsMyReservation);
        var user = await Session.GetUserAsync(TestData.AdministratorUserId);
        order.Should().NotBeNull();
        order.Type.Should().Be(OrderType.Owner);
        order.Owner!.Description.Should().Be(order.Owner!.Description);
        reservation.Status.Should().Be(ReservationStatus.Confirmed);
        ownerOrders.Should().Contain(o => o.OrderId == order.OrderId);
        ownerReservedDays.Should().Contain(reservation.ToMyReservedDays(order.OrderId, true));
        order.Audits.Select(audit => audit.Type).Should().Equal(OrderAuditType.PlaceOrder);
        user.Audits.Select(audit => audit.Type).Should().Contain(UserAuditType.CreateOwnerOrder);
    }

    [Fact]
    public async Task PlaceOrderThenCancelOrder()
    {
        var ownerOrder = await Session.PlaceOwnerOrderAsync(new TestReservation(TestData.Frederik.ResourceId));
        var order = await Session.CancelOwnerReservationAsync(ownerOrder.OrderId, 0);
        var reservation = order.Reservations.Single();
        var ownerOrders = await Session.GetOwnerOrdersAsync();
        var reservedDays = await Session.GetOwnerReservedDays();
        var ownerReservedDays = reservedDays.Where(reservedDay => reservedDay.IsMyReservation);
        order.Should().NotBeNull();
        order.Type.Should().Be(OrderType.Owner);
        order.Owner!.Description.Should().Be(ownerOrder.Owner!.Description);
        reservation.Status.Should().Be(ReservationStatus.Cancelled);
        ownerOrders.Should().NotContain(o => o.OrderId == order.OrderId);
        ownerReservedDays.Should().NotContain(reservation.ToMyReservedDays(order.OrderId, true));
        order.Audits.Select(audit => audit.Type).Should().Equal(OrderAuditType.PlaceOrder, OrderAuditType.CancelReservation, OrderAuditType.FinishOrder);
    }

    [Fact]
    public async Task PlaceOrderThenCancelOneReservation()
    {
        var ownerOrder = await Session.PlaceOwnerOrderAsync(
            new TestReservation(TestData.Frederik.ResourceId, 2),
            new TestReservation(TestData.Kaj.ResourceId));
        var confirmedReservation = ownerOrder.Reservations.Skip(1).First();
        var order = await Session.CancelOwnerReservationAsync(ownerOrder.OrderId, 0);
        var ownerOrders = await Session.GetOwnerOrdersAsync();
        var reservedDays = await Session.GetOwnerReservedDays();
        var ownerReservedDays = reservedDays.Where(reservedDay => reservedDay.IsMyReservation);
        order.Should().NotBeNull();
        order.Type.Should().Be(OrderType.Owner);
        order.Reservations.Select(reservation => reservation.Status).Should().Equal(ReservationStatus.Cancelled, ReservationStatus.Confirmed);
        ownerOrders.Should().Contain(o => o.OrderId == order.OrderId);
        ownerReservedDays.Should().Contain(confirmedReservation.ToMyReservedDays(order.OrderId, true));
    }

    [Fact]
    public async Task PlaceOrderThenUpdateDescription()
    {
        var ownerOrder = await Session.PlaceOwnerOrderAsync(new TestReservation(TestData.BanquetFacilities.ResourceId, 2));
        const string description = "This is a description";
        var order = await Session.UpdateOwnerOrderDescriptionAsync(ownerOrder.OrderId, description);
        order.Should().NotBeNull();
        order.Type.Should().Be(OrderType.Owner);
        order.Owner!.Description.Should().Be(description);
        order.Audits.Select(audit => audit.Type).Should().Equal(OrderAuditType.PlaceOrder, OrderAuditType.UpdateDescription);
    }

    [Fact]
    public async Task PlaceOrderThenCancelOneReservationAndUpdateDescription()
    {
        var ownerOrder = await Session.PlaceOwnerOrderAsync(
            new TestReservation(TestData.Frederik.ResourceId, 2),
            new TestReservation(TestData.Kaj.ResourceId));
        var confirmedReservation = ownerOrder.Reservations.Skip(1).First();
        const string description = "This is a description";
        var order = await Session.UpdateOwnerOrderDescriptionAndCancelReservationsAsync(ownerOrder.OrderId, description, 0);
        var ownerOrders = await Session.GetOwnerOrdersAsync();
        var reservedDays = await Session.GetOwnerReservedDays();
        var ownerReservedDays = reservedDays.Where(reservedDay => reservedDay.IsMyReservation);
        order.Should().NotBeNull();
        order.Type.Should().Be(OrderType.Owner);
        order.Owner!.Description.Should().Be(description);
        order.Reservations.Select(reservation => reservation.Status).Should().Equal(ReservationStatus.Cancelled, ReservationStatus.Confirmed);
        ownerOrders.Should().Contain(o => o.OrderId == order.OrderId);
        ownerReservedDays.Should().Contain(confirmedReservation.ToMyReservedDays(order.OrderId, true));
    }
}
