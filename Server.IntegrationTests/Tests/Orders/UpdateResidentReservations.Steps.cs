using FluentAssertions;
using Frederikskaj2.Reservations.Calendar;
using Frederikskaj2.Reservations.LockBox;
using Frederikskaj2.Reservations.Orders;
using Frederikskaj2.Reservations.Server.IntegrationTests.Harness;
using Frederikskaj2.Reservations.Users;
using LightBDD.Core.Extensibility.Execution;
using LightBDD.Framework;
using LightBDD.XUnit2;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Frederikskaj2.Reservations.Server.IntegrationTests.Tests.Orders;

sealed partial class UpdateResidentReservations(SessionFixture session) : FeatureFixture, IScenarioSetUp, IClassFixture<SessionFixture>
{
    State<MyOrderDto> order;
    State<HttpResponseMessage> response;

    MyOrderDto Order => order.GetValue(nameof(Order));
    MyReservationDto FrederikReservation => Order.Reservations.ElementAt(1);
    MyReservationDto KajReservation => Order.Reservations.ElementAt(2);
    HttpResponseMessage Response => response.GetValue(nameof(Response));

    async Task IScenarioSetUp.OnScenarioSetUp() => await session.UpdateLockBoxCodes();

    async Task GivenAConfirmedResidentOrder()
    {
        await session.SignUpAndSignIn();
        var placeMyOrderResponse = await session.PlaceResidentOrder(
            await CreateReservationRequest(SeedData.BanquetFacilities.ResourceId),
            await CreateReservationRequest(SeedData.Frederik.ResourceId),
            await CreateReservationRequest(SeedData.Kaj.ResourceId));
        await session.PayIn(placeMyOrderResponse.Order.Payment!.PaymentId, placeMyOrderResponse.Order.Price.Total());
        order = placeMyOrderResponse.Order;
        await session.RunConfirmOrders();
    }

    async Task GivenAnotherConfirmedResidentOrderRightAfterTheFirst()
    {
        var placeMyOrderResponse = await session.PlaceResidentOrder(await CreateReservationRequest(SeedData.Frederik.ResourceId));
        await session.PayIn(placeMyOrderResponse.Order.Payment!.PaymentId, placeMyOrderResponse.Order.Price.Total());
    }

    async Task WhenAnAdministratorUpdatesTheReservations(int frederikChange, int kajChange)
    {
        var frederikUpdate =
            new ReservationUpdateRequest(Extent: new(FrederikReservation.Extent.Date, FrederikReservation.Extent.Nights + frederikChange), ReservationIndex: 1);
        var kajUpdate = new ReservationUpdateRequest(Extent: new(KajReservation.Extent.Date, KajReservation.Extent.Nights + kajChange), ReservationIndex: 2);
        response = await session.UpdateResidentReservations(Order.OrderId, frederikUpdate, kajUpdate);
    }

    Task ThenTheUpdateSucceeds()
    {
        Response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        return Task.CompletedTask;
    }

    Task ThenTheUpdateFailsWithConflict()
    {
        Response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        return Task.CompletedTask;
    }

    async Task ThenTheResidentsOrderIsUpdated(int frederikChange, int kajChange)
    {
        var getMyOrdersResponse = await session.GetMyOrders();
        var myOrder = getMyOrdersResponse.Orders.Single();
        var updatedFrederikReservation = myOrder.Reservations.ElementAt(1);
        updatedFrederikReservation.Extent.Should().Be(new Extent(FrederikReservation.Extent.Date, FrederikReservation.Extent.Nights + frederikChange));
        var updatedKajReservation = myOrder.Reservations.ElementAt(2);
        updatedKajReservation.Extent.Should().Be(new Extent(KajReservation.Extent.Date, KajReservation.Extent.Nights + kajChange));
    }

    async Task ThenCalendarIsUpdated(int frederikChange, int kajChange)
    {
        var getReservedDaysResponse = await session.GetMyReservedDays();
        var residentReservedDays = getReservedDaysResponse.ReservedDays.Where(reservedDay => reservedDay.IsMyReservation);
        var partyFacilitiesReservedDays = Order.Reservations.First().ToMyReservedDays(Order.OrderId, isMyReservation: true);
        var frederikReservedDays = ReservationExtensions.GenerateReservedDays(
            FrederikReservation.Extent.Date,
            FrederikReservation.Extent.Nights + frederikChange,
            FrederikReservation.ResourceId,
            Order.OrderId,
            isMyReservation: true);
        var kajReservedDays = ReservationExtensions.GenerateReservedDays(
            KajReservation.Extent.Date, KajReservation.Extent.Nights + kajChange, KajReservation.ResourceId, Order.OrderId, isMyReservation: true);
        IEnumerable<ReservedDayDto> reservedDays = [..partyFacilitiesReservedDays, ..frederikReservedDays, ..kajReservedDays];
        residentReservedDays.Should().Contain(reservedDays);
    }

    async Task ThenTheResidentsBalanceIsNegative()
    {
        var getMyTransactionsResponse = await session.GetMyTransactions();
        var balance = getMyTransactionsResponse.Transactions.Select(transaction => transaction.Amount).Sum();
        balance.Should().BeLessThan(Amount.Zero);
    }

    async Task ThenTheResidentsBalanceIsPositive()
    {
        var getMyTransactionsResponse = await session.GetMyTransactions();
        var balance = getMyTransactionsResponse.Transactions.Select(transaction => transaction.Amount).Sum();
        balance.Should().BeGreaterThan(Amount.Zero);
    }

    async Task ThenTheUpdateIsAudited()
    {
        var getOrderResponse = await session.GetOrder(Order.OrderId);
        getOrderResponse.Order.Audits.Should().ContainEquivalentOf(new { UserId = SeedData.AdministratorUserId, Type = OrderAuditType.UpdateReservations });
    }

    async ValueTask<ReservationRequest> CreateReservationRequest(ResourceId resourceId) =>
        new(resourceId, await GetAvailableExtent(resourceId, 3));

    async ValueTask<Extent> GetAvailableExtent(ResourceId resourceId, int nights)
    {
        var getReservedDaysResponse = await session.GetOwnerReservedDays();
        var latestReservedDay = getReservedDaysResponse.ReservedDays
            .Where(reservedDay => reservedDay.ResourceId == resourceId)
            .Select(reservedDay => reservedDay.Date)
            .OrderDescending()
            .FirstOrDefault();
        if (latestReservedDay == default)
            latestReservedDay = session.CurrentDate.PlusDays(13);
        return new(latestReservedDay.PlusDays(1), nights);
    }
}
