using FluentAssertions;
using Frederikskaj2.Reservations.Orders;
using Frederikskaj2.Reservations.Server.IntegrationTests.Harness;
using LightBDD.Core.Extensibility.Execution;
using LightBDD.Framework;
using System.Linq;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Server.IntegrationTests.Tests.Cleaning;

sealed partial class SchedulingWithCancelledReservation(SessionFixture session) : CleaningFixture(session), IScenarioSetUp
{
    State<OrderDetailsDto> ownerOrder;
    State<ReservationDto> reservation1;
    State<ReservationDto> reservation2;

    OrderDetailsDto OwnerOrder => ownerOrder.GetValue(nameof(OwnerOrder));
    ReservationDto Reservation1 => reservation1.GetValue(nameof(Reservation1));
    ReservationDto Reservation2 => reservation2.GetValue(nameof(Reservation2));

    async Task IScenarioSetUp.OnScenarioSetUp() => await Session.UpdateLockBoxCodes();

    async Task GivenAResident() => await Session.SignUpAndSignIn();

    async Task GivenAConfirmedReservation()
    {
        var getMyOrderResponse = await Session.PlaceAndPayResidentOrder(new TestReservation(SeedData.Frederik.ResourceId));
        reservation1 = getMyOrderResponse.Order.Reservations.Single();
        await Session.RunConfirmOrders();
    }

    async Task GivenAnOwnerOrderWithTwoReservations()
    {
        var placeOwnerOrderResponse = await Session.PlaceOwnerOrder(
            new TestReservation(SeedData.Frederik.ResourceId),
            new TestReservation(SeedData.Frederik.ResourceId));
        reservation2 = placeOwnerOrderResponse.Order.Reservations.Last();
        ownerOrder = placeOwnerOrderResponse.Order;
    }

    async Task GivenTheFirstOwnerReservationIsCancelled() =>
        await Session.CancelOwnerReservation(OwnerOrder.OrderId, 0);

    Task ThenCleaningIsScheduledBetweenTheReservations()
    {
        var cleaningTask = GetCleaningTaskForReservation(CleaningTasks, Reservation1);
        cleaningTask.Should().NotBeNull();
        cleaningTask.End.Should().Be(Reservation2.Extent.Date.At(CheckinTime));
        (cleaningTask.End.Date - cleaningTask.Begin.Date).Days.Should().Be(IntervalInDays);
        return Task.CompletedTask;
    }
}
