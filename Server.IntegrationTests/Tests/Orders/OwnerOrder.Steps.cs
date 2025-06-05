using FluentAssertions;
using Frederikskaj2.Reservations.Cleaning;
using Frederikskaj2.Reservations.Orders;
using Frederikskaj2.Reservations.Server.IntegrationTests.Harness;
using Frederikskaj2.Reservations.Users;
using LightBDD.Framework;
using LightBDD.XUnit2;
using NodaTime;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Frederikskaj2.Reservations.Server.IntegrationTests.Tests.Orders;

partial class OwnerOrder(SessionFixture session) : FeatureFixture, IClassFixture<SessionFixture>
{
    const string description = "This is a description";

    static readonly LocalTime checkoutTime = new(10, 0);

    State<OrderDetailsDto> order;

    State<OrderId> orderId;
    State<IEnumerable<OrderSummaryDto>> ownerOrders;

    OrderId OrderId => orderId.GetValue(nameof(OrderId));
    OrderDetailsDto Order => order.GetValue(nameof(Order));
    IEnumerable<OrderSummaryDto> OwnerOrders => ownerOrders.GetValue(nameof(OwnerOrders));

    async Task GivenAnOwnerOrder()
    {
        var placeOwnerOrderResponse = await session.PlaceOwnerOrder(new TestReservation(SeedData.Frederik.ResourceId));
        orderId = placeOwnerOrderResponse.Order.OrderId;
    }

    async Task GivenAnOwnerOrderWithTwoReservations()
    {
        var placeOwnerOrderResponse = await session.PlaceOwnerOrder(
            new TestReservation(SeedData.Frederik.ResourceId, 2),
            new TestReservation(SeedData.Kaj.ResourceId));
        orderId = placeOwnerOrderResponse.Order.OrderId;
    }

    async Task WhenTheReservationIsCancelled() => await session.CancelOwnerReservation(OrderId, 0);

    async Task WhenTheDescriptionIsUpdated() => await session.UpdateOwnerOrderDescription(OrderId, description);

    async Task WhenCleaningIsNoLongerRequired() => await session.UpdateOwnerOrderCleaning(OrderId, isCleaningRequired: false);

    async Task WhenTheJobToFinishOwnerOrdersHasExecuted()
    {
        await session.FinishOwnerOrders();
        var getOrderResponse = await session.GetOrder(OrderId);
        order = getOrderResponse.Order;
        var getOwnerOrdersResponse = await session.GetOwnerOrders();
        ownerOrders = new(getOwnerOrdersResponse.Orders);
    }

    Task ThenTheOrderIsAnOwnerOrder()
    {
        Order.Type.Should().Be(OrderType.Owner);
        Order.Owner.Should().NotBeNull();
        return Task.CompletedTask;
    }

    Task ThenTheReservationIsConfirmed()
    {
        Order.Reservations.Single().Status.Should().Be(ReservationStatus.Confirmed);
        return Task.CompletedTask;
    }

    Task ThenTheReservationIsCancelled()
    {
        Order.Reservations.Single().Status.Should().Be(ReservationStatus.Cancelled);
        return Task.CompletedTask;
    }

    Task ThenTheOneReservationIsCancelledAndOneConfirmed()
    {
        Order.Reservations.Select(reservation => reservation.Status).Should().Equal(ReservationStatus.Cancelled, ReservationStatus.Confirmed);
        return Task.CompletedTask;
    }

    Task ThenTheDescriptionIsUpdated()
    {
        Order.Owner.Should().NotBeNull();
        Order.Owner!.Description.Should().Be(description);
        return Task.CompletedTask;
    }

    Task ThenCleaningIsRequired()
    {
        Order.Owner.Should().NotBeNull();
        Order.Owner!.IsCleaningRequired.Should().BeTrue();
        return Task.CompletedTask;
    }

    Task ThenCleaningIsNotRequired()
    {
        Order.Owner.Should().NotBeNull();
        Order.Owner!.IsCleaningRequired.Should().BeFalse();
        return Task.CompletedTask;
    }

    async Task ThenNoCleaningIsScheduled()
    {
        var cleaningTask = await GetCleaningTask();
        cleaningTask.Should().BeNull();
    }

    async Task ThenCleaningIsScheduled()
    {
        var cleaningTask = await GetCleaningTask();
        cleaningTask.Should().NotBeNull();
    }

    async Task<CleaningTask?> GetCleaningTask()
    {
        await session.UpdateCleaningSchedule();
        var getCleaningScheduleResponse = await session.GetCleaningSchedule();
        var reservation = Order.Reservations.First();
        var cleaningTask = getCleaningScheduleResponse.CleaningTasks.FirstOrDefault(
            task => task.ResourceId == reservation.ResourceId && task.Begin == reservation.Extent.Ends().At(checkoutTime));
        return cleaningTask;
    }

    Task ThenTheOwnerOrdersContainTheOrder()
    {
        OwnerOrders.Should().Contain(ownerOrder => ownerOrder.OrderId == Order.OrderId);
        return Task.CompletedTask;
    }

    Task ThenTheOwnerOrdersDoesNotContainTheOrder()
    {
        OwnerOrders.Should().NotContain(ownerOrder => ownerOrder.OrderId == Order.OrderId);
        return Task.CompletedTask;
    }

    async Task ThenTheOrderIsReservedInTheCalendar()
    {
        var getReservedDaysResponse = await session.GetOwnerReservedDays();
        var ownerReservedDays = getReservedDaysResponse.ReservedDays.Where(reservedDay => reservedDay.IsMyReservation);
        var reservedDays = Order.Reservations
            .Where(reservation => reservation.Status is ReservationStatus.Confirmed)
            .SelectMany(reservation => reservation.ToMyReservedDays(Order.OrderId, isMyReservation: true));
        ownerReservedDays.Should().Contain(reservedDays);
    }

    async Task ThenTheOrderIsNotReservedInTheCalendar()
    {
        var getReservedDaysResponse = await session.GetOwnerReservedDays();
        var ownerReservedDays = getReservedDaysResponse.ReservedDays.Where(reservedDay => reservedDay.IsMyReservation);
        var reservedDays = Order.Reservations
            .Where(reservation => reservation.Status is not ReservationStatus.Confirmed)
            .SelectMany(reservation => reservation.ToMyReservedDays(Order.OrderId, isMyReservation: true));
        ownerReservedDays.Should().NotContain(reservedDays);
    }

    Task ThenTheOrderPlacementIsAudited()
    {
        Order.Audits.Should().BeEquivalentTo([
            new
            {
                UserId = SeedData.AdministratorUserId,
                Type = OrderAuditType.PlaceOrder,
            },
        ]);
        return Task.CompletedTask;
    }

    Task ThenTheOrderCancellationIsAudited()
    {
        Order.Audits.Should().BeEquivalentTo([
            new
            {
                UserId = (UserId?) SeedData.AdministratorUserId,
                Type = OrderAuditType.PlaceOrder,
            },
            new
            {
                UserId = (UserId?) SeedData.AdministratorUserId,
                Type = OrderAuditType.CancelReservation,
            },
            new
            {
                UserId = (UserId?) null,
                Type = OrderAuditType.FinishOrder,
            },
        ]);
        return Task.CompletedTask;
    }

    Task ThenTheDescriptionUpdateIsAudited()
    {
        Order.Audits.Should().BeEquivalentTo([
            new
            {
                UserId = (UserId?) SeedData.AdministratorUserId,
                Type = OrderAuditType.PlaceOrder,
            },
            new
            {
                UserId = (UserId?) SeedData.AdministratorUserId,
                Type = OrderAuditType.UpdateDescription,
            },
        ]);
        return Task.CompletedTask;
    }

    Task ThenTheCancellationAndDescriptionUpdateAreAudited()
    {
        Order.Audits.Should().BeEquivalentTo([
            new
            {
                UserId = (UserId?) SeedData.AdministratorUserId,
                Type = OrderAuditType.PlaceOrder,
            },
            new
            {
                UserId = (UserId?) SeedData.AdministratorUserId,
                Type = OrderAuditType.CancelReservation,
            },
            new
            {
                UserId = (UserId?) SeedData.AdministratorUserId,
                Type = OrderAuditType.UpdateDescription,
            },
        ]);
        return Task.CompletedTask;
    }

    Task ThenTheCleaningUpdateIsAudited()
    {
        Order.Audits.Should().BeEquivalentTo([
            new
            {
                UserId = (UserId?) SeedData.AdministratorUserId,
                Type = OrderAuditType.PlaceOrder,
            },
            new
            {
                UserId = (UserId?) SeedData.AdministratorUserId,
                Type = OrderAuditType.UpdateCleaning,
            },
        ]);
        return Task.CompletedTask;
    }

    async Task ThenTheOrderPlacementIsAuditedForTheUser()
    {
        var getUserResponse = await session.GetUser(SeedData.AdministratorUserId);
        getUserResponse.User.Audits.Should().ContainEquivalentOf(
            new
            {
                UserId = SeedData.AdministratorUserId,
                Order.OrderId,
                Type = UserAuditType.PlaceOwnerOrder,
            });
    }
}
