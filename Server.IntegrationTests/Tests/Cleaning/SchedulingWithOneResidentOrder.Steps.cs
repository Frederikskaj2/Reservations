using FluentAssertions;
using Frederikskaj2.Reservations.Orders;
using Frederikskaj2.Reservations.Server.IntegrationTests.Harness;
using LightBDD.Core.Extensibility.Execution;
using LightBDD.Framework;
using System.Linq;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Server.IntegrationTests.Tests.Cleaning;

sealed partial class SchedulingWithOneResidentOrder(SessionFixture session) : CleaningFixture(session), IScenarioSetUp
{
    State<MyOrderDto> order;

    MyOrderDto Order => order.GetValue(nameof(Order));

    async Task IScenarioSetUp.OnScenarioSetUp() => await Session.UpdateLockBoxCodes();

    async Task GivenAnOrderWithOneConfirmedReservation()
    {
        await Session.SignUpAndSignIn();
        var getMyOrderResponse = await Session.PlaceAndPayResidentOrder(new TestReservation(SeedData.Frederik.ResourceId));
        order = getMyOrderResponse.Order;
        await Session.RunConfirmOrders();
    }

    async Task GivenAnOrderWithTwoConfirmedReservations()
    {
        await Session.SignUpAndSignIn();
        var getMyOrderResponse = await Session.PlaceAndPayResidentOrder(
            new TestReservation(SeedData.Kaj.ResourceId),
            new TestReservation(SeedData.Kaj.ResourceId, 1, IntervalInDays));
        order = getMyOrderResponse.Order;
        await Session.RunConfirmOrders();
    }

    async Task GivenAnOrderWithOneConfirmedAndOneCancelledReservation()
    {
        await Session.SignUpAndSignIn();
        var getMyOrderResponse = await Session.PlaceAndPayResidentOrder(
            new TestReservation(SeedData.Kaj.ResourceId),
            new TestReservation(SeedData.Kaj.ResourceId, 1, IntervalInDays));
        order = getMyOrderResponse.Order;
        await Session.RunConfirmOrders();
        await Session.CancelResidentReservations(Order.OrderId, 1);
    }

    Task ThenCleaningIsScheduledAfterTheReservation()
    {
        var reservation = Order.Reservations.Single();
        var cleaningTask = GetCleaningTaskForReservation(CleaningTasks, reservation);
        cleaningTask.Should().NotBeNull();
        cleaningTask.End.Should().Be(reservation.Extent.Ends().PlusDays(AdditionalDaysWhereCleaningCanBeDone).At(CheckinTime));
        return Task.CompletedTask;
    }

    Task ThenCleaningIsScheduledBetweenTheReservations()
    {
        var reservation1 = Order.Reservations.First();
        var reservation2 = Order.Reservations.Last();
        var cleaningTask = GetCleaningTaskForReservation(CleaningTasks, reservation1);
        cleaningTask.Should().NotBeNull();
        cleaningTask.End.Should().Be(reservation2.Extent.Date.At(CheckinTime));
        (cleaningTask.End.Date - cleaningTask.Begin.Date).Days.Should().Be(IntervalInDays);
        return Task.CompletedTask;
    }

    Task ThenCleaningIsScheduledAfterTheFirstReservationIgnoringTheSecond()
    {
        var reservation1 = Order.Reservations.First();
        var reservation2 = Order.Reservations.Last();
        var cleaningTask1 = GetCleaningTaskForReservation(CleaningTasks, reservation1);
        var cleaningTask2 = GetCleaningTaskForReservation(CleaningTasks, reservation2);
        cleaningTask1.Should().NotBeNull();
        cleaningTask1.End.Should().Be(reservation1.Extent.Ends().PlusDays(AdditionalDaysWhereCleaningCanBeDone).At(CheckinTime));
        (cleaningTask1.End.Date - cleaningTask1.Begin.Date).Days.Should().Be(AdditionalDaysWhereCleaningCanBeDone);
        cleaningTask2.Should().BeNull();
        return Task.CompletedTask;
    }
}
