using FluentAssertions;
using Frederikskaj2.Reservations.Orders;
using Frederikskaj2.Reservations.Server.IntegrationTests.Harness;
using LightBDD.Core.Extensibility.Execution;
using LightBDD.Framework;
using System.Linq;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Server.IntegrationTests.Tests.Cleaning;

sealed partial class SchedulingWithTwoOrders(SessionFixture session) : CleaningFixture(session), IScenarioSetUp
{
    async Task IScenarioSetUp.OnScenarioSetUp() => await Session.UpdateLockBoxCodes();

    State<ReservationDto> reservation1;
    State<ReservationDto> reservation2;

    ReservationDto Reservation1 => reservation1.GetValue(nameof(Reservation1));
    ReservationDto Reservation2 => reservation2.GetValue(nameof(Reservation2));

    async Task GivenAResident() => await Session.SignUpAndSignIn();

    async Task GivenAConfirmedReservation()
    {
        var getMyOrderResponse1 = await Session.PlaceAndPayResidentOrder(new TestReservation(SeedData.Frederik.ResourceId));
        reservation1 = getMyOrderResponse1.Order.Reservations.Single();
        await Session.RunConfirmOrders();
    }

    async Task GivenAnOwnerReservation()
    {
        var placeOwnerOrderResponse1 = await Session.PlaceOwnerOrder(new TestReservation(SeedData.Frederik.ResourceId));
        reservation1 = placeOwnerOrderResponse1.Order.Reservations.Single();
    }

    async Task GivenAnotherConfirmedReservation()
    {
        var getMyOrderResponse2 = await Session.PlaceAndPayResidentOrder(new TestReservation(SeedData.Frederik.ResourceId, 1, IntervalInDays));
        reservation2 = getMyOrderResponse2.Order.Reservations.Single();
        await Session.RunConfirmOrders();
    }

    async Task GivenAnotherReservedReservation()
    {
        var placeMyOrderResponse2 = await Session.PlaceResidentOrder(new TestReservation(SeedData.Frederik.ResourceId, 1, IntervalInDays));
        reservation2 = placeMyOrderResponse2.Order.Reservations.Single();
    }

    async Task GivenAnotherOwnerReservation()
    {
        var placeOwnerOrderResponse2 = await Session.PlaceOwnerOrder(new TestReservation(SeedData.Frederik.ResourceId, 1, IntervalInDays));
        reservation2 = placeOwnerOrderResponse2.Order.Reservations.Single();
    }

    Task ThenCleaningIsScheduledBetweenTheReservations()
    {
        var cleaningTask = GetCleaningTaskForReservation(CleaningTasks, Reservation1);
        cleaningTask.Should().NotBeNull();
        cleaningTask.End.Should().Be(Reservation2.Extent.Date.At(CheckinTime));
        (cleaningTask.End.Date - cleaningTask.Begin.Date).Days.Should().Be(IntervalInDays);
        return Task.CompletedTask;
    }

    Task ThenCleaningIsScheduledAfterTheFirstReservationIgnoringTheSecond()
    {
        var cleaningTask1 = GetCleaningTaskForReservation(CleaningTasks, Reservation1);
        var cleaningTask2 = GetCleaningTaskForReservation(CleaningTasks, Reservation2);
        cleaningTask1.Should().NotBeNull();
        cleaningTask1.End.Should().Be(Reservation1.Extent.Ends().PlusDays(AdditionalDaysWhereCleaningCanBeDone).At(CheckinTime));
        (cleaningTask1.End.Date - cleaningTask1.Begin.Date).Days.Should().Be(AdditionalDaysWhereCleaningCanBeDone);
        cleaningTask2.Should().BeNull();
        return Task.CompletedTask;
    }
}
