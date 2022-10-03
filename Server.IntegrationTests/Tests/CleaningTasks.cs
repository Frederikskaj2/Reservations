using FluentAssertions;
using Frederikskaj2.Reservations.Server.IntegrationTests.Harness;
using Frederikskaj2.Reservations.Shared.Core;
using NodaTime;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Frederikskaj2.Reservations.Server.IntegrationTests.Tests;

public class CleaningTasks : IClassFixture<SessionFixture>
{
    // Notice that the cleaning schedule is only 45 days. These tests
    // depend on all the cleaning tasks being made here are within that
    // limit.
    const int additionalDaysWhereCleaningCanBeDone = 3;
    static readonly LocalTime checkoutTime = new(10, 0);
    static readonly LocalTime checkinTime = new(12, 0);

    public CleaningTasks(SessionFixture session) => Session = session;

    SessionFixture Session { get; }

    [Fact]
    public async Task OneConfirmedReservation()
    {
        await Session.SignUpAndSignInAsync();
        var userOrder = await Session.UserPlaceAndPayOrderAsync(new TestReservation(TestData.Frederik.ResourceId));
        var cleaningSchedule = await Session.GetCleaningScheduleAsync();
        var reservation = userOrder.Reservations!.Single();
        var cleaningTask = GetCleaningTaskForReservation(cleaningSchedule.CleaningTasks, reservation);
        cleaningTask.Should().NotBeNull();
        cleaningTask!.End.Should().Be(reservation.Extent.Ends().PlusDays(additionalDaysWhereCleaningCanBeDone).At(checkinTime));
    }

    [Fact]
    public async Task OneOrderWithTwoConfirmedReservations()
    {
        await Session.SignUpAndSignInAsync();
        const int intervalInDays = 1;
        var userOrder = await Session.UserPlaceAndPayOrderAsync(
            new TestReservation(TestData.Kaj.ResourceId),
            new TestReservation(TestData.Kaj.ResourceId, 1, intervalInDays));
        var cleaningSchedule = await Session.GetCleaningScheduleAsync();
        var reservation1 = userOrder.Reservations!.First();
        var reservation2 = userOrder.Reservations!.Last();
        var cleaningTask = GetCleaningTaskForReservation(cleaningSchedule.CleaningTasks, reservation1);
        cleaningTask.Should().NotBeNull();
        cleaningTask!.End.Should().Be(reservation2.Extent.Date.At(checkinTime));
        (cleaningTask.End.Date - cleaningTask.Begin.Date).Days.Should().Be(intervalInDays);
    }

    [Fact]
    public async Task TwoOrdersWithTwoConfirmedReservations()
    {
        await Session.SignUpAndSignInAsync();
        var userOrder1 = await Session.UserPlaceAndPayOrderAsync(new TestReservation(TestData.Frederik.ResourceId));
        const int intervalInDays = 1;
        var userOrder2 = await Session.UserPlaceAndPayOrderAsync(new TestReservation(TestData.Frederik.ResourceId, 1, intervalInDays));
        var cleaningSchedule = await Session.GetCleaningScheduleAsync();
        var reservation1 = userOrder1.Reservations!.Single();
        var reservation2 = userOrder2.Reservations!.Single();
        var cleaningTask = GetCleaningTaskForReservation(cleaningSchedule.CleaningTasks, reservation1);
        cleaningTask.Should().NotBeNull();
        cleaningTask!.End.Should().Be(reservation2.Extent.Date.At(checkinTime));
        (cleaningTask.End.Date - cleaningTask.Begin.Date).Days.Should().Be(intervalInDays);
    }

    [Fact]
    public async Task OneConfirmedReservationOneReservedReservation()
    {
        await Session.SignUpAndSignInAsync();
        var userOrder1 = await Session.UserPlaceAndPayOrderAsync(new TestReservation(TestData.Kaj.ResourceId));
        const int intervalInDays = 1;
        var userOrder2 = await Session.UserPlaceOrderAsync(new TestReservation(TestData.Kaj.ResourceId, 1, intervalInDays));
        var cleaningSchedule = await Session.GetCleaningScheduleAsync();
        var reservation1 = userOrder1.Reservations!.Single();
        var cleaningTask = GetCleaningTaskForReservation(cleaningSchedule.CleaningTasks, reservation1);
        cleaningTask.Should().NotBeNull();
        cleaningTask!.End.Should().Be(reservation1.Extent.Ends().PlusDays(additionalDaysWhereCleaningCanBeDone).At(checkinTime));
        (cleaningTask.End.Date - cleaningTask.Begin.Date).Days.Should().Be(additionalDaysWhereCleaningCanBeDone);
        // Clean up to avoid later payments to be consumed by this order.
        await Session.UserCancelReservationsAsync(userOrder2.OrderId, 0);
    }

    [Fact]
    public async Task OneConfirmedReservationOneCancelledReservation()
    {
        await Session.SignUpAndSignInAsync();
        const int intervalInDays = 1;
        var userOrder = await Session.UserPlaceAndPayOrderAsync(
            new TestReservation(TestData.Frederik.ResourceId),
            new TestReservation(TestData.Frederik.ResourceId, 1, intervalInDays));
        var reservation1 = userOrder.Reservations!.First();
        var reservation2 = userOrder.Reservations!.Last();
        await Session.UserCancelReservationsAsync(userOrder.OrderId, userOrder.Reservations!.Count() - 1);
        var cleaningSchedule = await Session.GetCleaningScheduleAsync();
        var cleaningTask1 = GetCleaningTaskForReservation(cleaningSchedule.CleaningTasks, reservation1);
        var cleaningTask2 = GetCleaningTaskForReservation(cleaningSchedule.CleaningTasks, reservation2);
        cleaningTask1.Should().NotBeNull();
        cleaningTask1!.End.Should().Be(reservation1.Extent.Ends().PlusDays(additionalDaysWhereCleaningCanBeDone).At(checkinTime));
        (cleaningTask1.End.Date - cleaningTask1.Begin.Date).Days.Should().Be(additionalDaysWhereCleaningCanBeDone);
        cleaningTask2.Should().BeNull();
    }

    [Fact]
    public async Task OneOwnerReservation()
    {
        var ownerOrder = await Session.PlaceOwnerOrderAsync(new TestReservation(TestData.Kaj.ResourceId));
        var cleaningSchedule = await Session.GetCleaningScheduleAsync();
        var reservation = ownerOrder.Reservations.Single();
        var cleaningTask = GetCleaningTaskForReservation(cleaningSchedule.CleaningTasks, reservation);
        cleaningTask.Should().NotBeNull();
        cleaningTask!.End.Should().Be(reservation.Extent.Ends().PlusDays(additionalDaysWhereCleaningCanBeDone).At(checkinTime));
    }

    [Fact]
    public async Task OneOwnerReservationWithoutCleaning()
    {
        var ownerOrder = await Session.PlaceOwnerOrderAsync(false, new TestReservation(TestData.Frederik.ResourceId));
        var cleaningSchedule = await Session.GetCleaningScheduleAsync();
        var reservation = ownerOrder.Reservations.Single();
        var cleaningTask = GetCleaningTaskForReservation(cleaningSchedule.CleaningTasks, reservation);
        cleaningTask.Should().BeNull();
    }

    [Fact]
    public async Task OneOwnerReservationWithCleaningRemoved()
    {
        var ownerOrder = await Session.PlaceOwnerOrderAsync(new TestReservation(TestData.Kaj.ResourceId));
        await Session.UpdateOwnerOrderCleaningAsync(ownerOrder.OrderId, false);
        var cleaningSchedule = await Session.GetCleaningScheduleAsync();
        var reservation = ownerOrder.Reservations.Single();
        var cleaningTask = GetCleaningTaskForReservation(cleaningSchedule.CleaningTasks, reservation);
        cleaningTask.Should().BeNull();
    }

    [Fact]
    public async Task OneOwnerReservationWithCleaningAdded()
    {
        var ownerOrder = await Session.PlaceOwnerOrderAsync(false, new TestReservation(TestData.Frederik.ResourceId));
        await Session.UpdateOwnerOrderCleaningAsync(ownerOrder.OrderId, true);
        var cleaningSchedule = await Session.GetCleaningScheduleAsync();
        var reservation = ownerOrder.Reservations.Single();
        var cleaningTask = GetCleaningTaskForReservation(cleaningSchedule.CleaningTasks, reservation);
        cleaningTask.Should().NotBeNull();
        cleaningTask!.End.Should().Be(reservation.Extent.Ends().PlusDays(additionalDaysWhereCleaningCanBeDone).At(checkinTime));
    }

    [Fact]
    public async Task OneOwnerReservationAndOneConfirmedReservation()
    {
        var ownerOrder = await Session.PlaceOwnerOrderAsync(new TestReservation(TestData.Kaj.ResourceId));
        await Session.SignUpAndSignInAsync();
        await Session.UserPlaceAndPayOrderAsync(new TestReservation(TestData.Kaj.ResourceId));
        var cleaningSchedule = await Session.GetCleaningScheduleAsync();
        var reservation = ownerOrder.Reservations.Single();
        var cleaningTask = GetCleaningTaskForReservation(cleaningSchedule.CleaningTasks, reservation);
        cleaningTask.Should().NotBeNull();
        cleaningTask!.End.Should().Be(reservation.Extent.Ends().At(checkinTime));
    }

    [Fact]
    public async Task TwoOwnerReservations()
    {
        var ownerOrder1 = await Session.PlaceOwnerOrderAsync(new TestReservation(TestData.Frederik.ResourceId));
        await Session.PlaceOwnerOrderAsync(new TestReservation(TestData.Frederik.ResourceId));
        var cleaningSchedule = await Session.GetCleaningScheduleAsync();
        var reservation = ownerOrder1.Reservations.Single();
        var cleaningTask = GetCleaningTaskForReservation(cleaningSchedule.CleaningTasks, reservation);
        cleaningTask.Should().NotBeNull();
        cleaningTask!.End.Should().Be(reservation.Extent.Ends().At(checkinTime));
    }

    [Fact]
    public async Task OneConfirmedReservationWithOwnerReservationCancelled()
    {
        await Session.SignUpAndSignInAsync();
        var userOrder = await Session.UserPlaceAndPayOrderAsync(new TestReservation(TestData.Kaj.ResourceId));
        var ownerOrder = await Session.PlaceOwnerOrderAsync(new TestReservation(TestData.Kaj.ResourceId), new TestReservation(TestData.Kaj.ResourceId));
        var ownerReservation2 = ownerOrder.Reservations.Last();
        await Session.CancelOwnerReservationAsync(ownerOrder.OrderId, 0);
        var cleaningSchedule = await Session.GetCleaningScheduleAsync();
        var reservation = userOrder.Reservations!.Single();
        var cleaningTask = GetCleaningTaskForReservation(cleaningSchedule.CleaningTasks, reservation);
        cleaningTask.Should().NotBeNull();
        cleaningTask!.End.Should().Be(reservation.Extent.Ends().PlusDays(1).At(checkinTime));
        (cleaningTask.End.Date - cleaningTask.Begin.Date).Days.Should().Be(1);
        cleaningTask.End.Date.Should().Be(ownerReservation2.Extent.Date);
    }

    [Fact]
    public async Task OneConfirmedReservationInThePastWithCleaningInThePast()
    {
        await Session.SignUpAndSignInAsync();
        var userOrder = await Session.UserPlaceAndPayOrderAsync(new TestReservation(TestData.Frederik.ResourceId));
        var reservation = userOrder.Reservations!.Single();
        Session.NowOffset += reservation.Extent.Ends().PlusDays(additionalDaysWhereCleaningCanBeDone + 1) - Session.CurrentDate;
        var cleaningSchedule = await Session.GetCleaningScheduleAsync();
        var cleaningTask = GetCleaningTaskForReservation(cleaningSchedule.CleaningTasks, reservation);
        cleaningTask.Should().BeNull();
    }

    [Fact]
    public async Task OneConfirmedReservationInThePastWithCleaningInTheFuture()
    {
        await Session.SignUpAndSignInAsync();
        var userOrder = await Session.UserPlaceAndPayOrderAsync(new TestReservation(TestData.Frederik.ResourceId));
        var reservation = userOrder.Reservations!.Single();
        Session.NowOffset += reservation.Extent.Ends().PlusDays(additionalDaysWhereCleaningCanBeDone) - Session.CurrentDate;
        var cleaningSchedule = await Session.GetCleaningScheduleAsync();
        var cleaningTask = GetCleaningTaskForReservation(cleaningSchedule.CleaningTasks, reservation);
        cleaningTask.Should().NotBeNull();
    }

    [Fact]
    public async Task OneOwnerReservationInThePastWithCleaningInThePast()
    {
        var ownerOrder = await Session.PlaceOwnerOrderAsync(new TestReservation(TestData.Kaj.ResourceId));
        var reservation = ownerOrder.Reservations.Single();
        Session.NowOffset += reservation.Extent.Ends().PlusDays(additionalDaysWhereCleaningCanBeDone + 1) - Session.CurrentDate;
        await Session.AdministratorGetAsync("orders/owner");
        var cleaningSchedule = await Session.GetCleaningScheduleAsync();
        var cleaningTask = GetCleaningTaskForReservation(cleaningSchedule.CleaningTasks, reservation);
        cleaningTask.Should().BeNull();
    }

    [Fact]
    public async Task OneOwnerReservationInThePastWithCleaningInTheFuture()
    {
        var ownerOrder = await Session.PlaceOwnerOrderAsync(new TestReservation(TestData.Kaj.ResourceId));
        var reservation = ownerOrder.Reservations.Single();
        Session.NowOffset += reservation.Extent.Ends().PlusDays(additionalDaysWhereCleaningCanBeDone) - Session.CurrentDate;
        await Session.AdministratorGetAsync("orders/owner");
        var cleaningSchedule = await Session.GetCleaningScheduleAsync();
        var cleaningTask = GetCleaningTaskForReservation(cleaningSchedule.CleaningTasks, reservation);
        cleaningTask.Should().NotBeNull();
    }

    static CleaningTask? GetCleaningTaskForReservation(IEnumerable<CleaningTask> cleaningTasks, Reservation reservation) =>
        cleaningTasks.FirstOrDefault(task => task.ResourceId == reservation.ResourceId && task.Begin == reservation.Extent.Ends().At(checkoutTime));
}
