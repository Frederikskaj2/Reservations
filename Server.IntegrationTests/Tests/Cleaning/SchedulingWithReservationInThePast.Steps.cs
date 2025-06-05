using FluentAssertions;
using Frederikskaj2.Reservations.Orders;
using Frederikskaj2.Reservations.Server.IntegrationTests.Harness;
using LightBDD.Core.Extensibility.Execution;
using LightBDD.Framework;
using NodaTime;
using System.Linq;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Server.IntegrationTests.Tests.Cleaning;

sealed partial class SchedulingWithReservationInThePast(SessionFixture session) : CleaningFixture(session), IScenarioSetUp
{
    const int additionalDaysWhereCleaningCanBeDone = 3;

    State<ReservationDto> reservation;
    ReservationDto Reservation => reservation.GetValue(nameof(Reservation));

    async Task IScenarioSetUp.OnScenarioSetUp()
    {
        Session.NowOffset = Period.Zero;
        await Session.UpdateLockBoxCodes();
    }

    async Task GivenAResident() => await Session.SignUpAndSignIn();

    async Task GivenAConfirmedReservation()
    {
        var getMyOrderResponse = await Session.PlaceAndPayResidentOrder(new TestReservation(SeedData.Frederik.ResourceId));
        reservation = getMyOrderResponse.Order.Reservations.Single();
        await Session.ConfirmOrders();
    }

    async Task GivenAnOwnerReservation()
    {
        var placeOwnerOrderResponse1 = await Session.PlaceOwnerOrder(new TestReservation(SeedData.Frederik.ResourceId));
        reservation = placeOwnerOrderResponse1.Order.Reservations.Single();
    }

    Task GivenTheScheduledCleaningStartsInThePast()
    {
        Session.NowOffset += Reservation.Extent.Ends().PlusDays(additionalDaysWhereCleaningCanBeDone + 1) - Session.CurrentDate;
        return Task.CompletedTask;
    }

    Task GivenTheScheduledCleaningStartsInTheFuture()
    {
        Session.NowOffset += Reservation.Extent.Ends().PlusDays(additionalDaysWhereCleaningCanBeDone) - Session.CurrentDate;
        return Task.CompletedTask;
    }

    Task ThenCleaningIsScheduled()
    {
        var cleaningTask = GetCleaningTaskForReservation(CleaningTasks, reservation);
        cleaningTask.Should().NotBeNull();
        return Task.CompletedTask;
    }

    Task ThenCleaningIsNotScheduled()
    {
        var cleaningTask = GetCleaningTaskForReservation(CleaningTasks, reservation);
        cleaningTask.Should().BeNull();
        return Task.CompletedTask;
    }
}
