using Frederikskaj2.Reservations.Shared.Core;
using Frederikskaj2.Reservations.Shared.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Server.IntegrationTests.Harness;

static class UserOrderExtensions
{
    public static async ValueTask<IEnumerable<MyReservedDay>> GetUserReservedDays(this SessionFixture session) =>
        await session.DeserializeAsync<IEnumerable<MyReservedDay>>(await session.GetAsync("reserved-days/my"));

    public static async ValueTask<IEnumerable<MyReservedDay>> GetOwnerReservedDays(this SessionFixture session) =>
        await session.DeserializeAsync<IEnumerable<MyReservedDay>>(await session.AdministratorGetAsync("reserved-days/owner"));

    public static async ValueTask<MyOrder> GetMyOrderAsync(this SessionFixture session, OrderId orderId) =>
        await session.DeserializeAsync<MyOrder>(await session.GetAsync($"orders/my/{orderId}"));

    public static async ValueTask<MyUser> GetMyUserAsync(this SessionFixture session) =>
        await session.DeserializeAsync<MyUser>(await session.GetAsync("user"));

    public static async ValueTask<MyOrder> UserPlaceOrderAsync(this SessionFixture session, params TestReservation[] reservations)
    {
        if (session.User is null)
            throw new InvalidOperationException();

        var request = new PlaceMyOrderRequest
        {
            FullName = session.User.FullName,
            Phone = session.User.Phone,
            ApartmentId = session.User.ApartmentId,
            AccountNumber = session.User.AccountNumber,
            Reservations = reservations.Select(reservation => new ReservationRequest
            {
                ResourceId = reservation.ResourceId,
                Extent = session.Calendar.GetAvailableExtent(session, reservation)
            })
        };
        return await session.DeserializeAsync<MyOrder>(await session.PostAsync("orders/my", request));
    }

    public static async ValueTask<MyOrder> UserPlaceOrderAsync(this SessionFixture session, params Reservation[] reservations)
    {
        if (session.User is null)
            throw new InvalidOperationException();

        var request = new PlaceMyOrderRequest
        {
            FullName = session.User.FullName,
            Phone = session.User.Phone,
            ApartmentId = session.User.ApartmentId,
            AccountNumber = session.User.AccountNumber,
            Reservations = reservations.Select(reservation => new ReservationRequest
            {
                ResourceId = reservation.ResourceId,
                Extent = reservation.Extent
            })
        };
        return await session.DeserializeAsync<MyOrder>(await session.PostAsync("orders/my", request));
    }

    public static async ValueTask<MyOrder> UserPlaceAndPayOrderAsync(this SessionFixture session, params TestReservation[] reservations)
    {
        var userOrder = await session.UserPlaceOrderAsync(reservations);
        await session.PayInAsync(userOrder.Payment!.PaymentId, userOrder.Price.Total());
        return await session.GetMyOrderAsync(userOrder.OrderId);
    }

    public static async ValueTask<UpdateMyOrderResult> UserCancelReservationsAsync(
        this SessionFixture session, OrderId orderId, params ReservationIndex[] reservationIndices) =>
        await session.DeserializeAsync<UpdateMyOrderResult>(await session.CancelUserReservationRawAsync(orderId, reservationIndices));

    public static ValueTask<HttpResponseMessage> CancelUserReservationRawAsync(
        this SessionFixture session, OrderId orderId, params ReservationIndex[] reservationIndices)
    {
        if (session.User is null)
            throw new InvalidOperationException();

        var request = new UpdateMyOrderRequest { CancelledReservations = reservationIndices.ToHashSet() };
        return session.PatchAsync($"orders/my/{orderId}", request);
    }

    public static async ValueTask<UpdateMyOrderResult> CancelUserReservationNoFeeAsync(
        this SessionFixture session, OrderId orderId, params ReservationIndex[] reservationIndices)
    {
        if (session.User is null)
            throw new InvalidOperationException();

        var request = new UpdateMyOrderRequest
        {
            AccountNumber = session.User.AccountNumber,
            CancelledReservations = reservationIndices.ToHashSet(),
            WaiveFee = true
        };
        return await session.DeserializeAsync<UpdateMyOrderResult>(await session.PatchAsync($"orders/my/{orderId}", request));
    }

    public static async ValueTask<UpdateMyOrderResult> UserUpdateAccountNumberAsync(
        this SessionFixture session, OrderId orderId, string accountNumber)
    {
        if (session.User is null)
            throw new InvalidOperationException();

        var request = new UpdateMyOrderRequest { AccountNumber = accountNumber};
        return await session.DeserializeAsync<UpdateMyOrderResult>(await session.PatchAsync($"orders/my/{orderId}", request));
    }
}
