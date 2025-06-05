using FluentAssertions;
using Frederikskaj2.Reservations.Orders;
using Frederikskaj2.Reservations.Server.IntegrationTests.Harness;
using LightBDD.Framework;
using System.Linq;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Server.IntegrationTests.Tests.Cleaning;

partial class SchedulingWithOneOwnerOrder(SessionFixture session) : CleaningFixture(session)
{
    State<OrderDetailsDto> order;

    OrderDetailsDto Order => order.GetValue(nameof(Order));

    async Task GivenAnOrderWithOneReservationWithCleaning()
    {
        var placeOwnerOrderResponse = await Session.PlaceOwnerOrder(new TestReservation(SeedData.Kaj.ResourceId));
        order = placeOwnerOrderResponse.Order;
    }

    async Task GivenAnOrderWithOneReservationWithoutCleaning()
    {
        var placeOwnerOrderResponse = await Session.PlaceOwnerOrder(isCleaningRequired: false, new TestReservation(SeedData.Kaj.ResourceId));
        order = placeOwnerOrderResponse.Order;
    }

    async Task GivenCleaningIsRemoved() => await Session.UpdateOwnerOrderCleaning(Order.OrderId, isCleaningRequired: false);

    async Task GivenCleaningIsAdded() => await Session.UpdateOwnerOrderCleaning(Order.OrderId, isCleaningRequired: true);

    Task ThenCleaningIsScheduledAfterTheReservation()
    {
        var reservation = Order.Reservations.Single();
        var cleaningTask = GetCleaningTaskForReservation(CleaningTasks, reservation);
        cleaningTask.Should().NotBeNull();
        cleaningTask.End.Should().Be(reservation.Extent.Ends().PlusDays(AdditionalDaysWhereCleaningCanBeDone).At(CheckinTime));
        return Task.CompletedTask;
    }

    Task ThenNoCleaningIsScheduled()
    {
        var reservation = Order.Reservations.Single();
        var cleaningTask = GetCleaningTaskForReservation(CleaningTasks, reservation);
        cleaningTask.Should().BeNull();
        return Task.CompletedTask;
    }
}
