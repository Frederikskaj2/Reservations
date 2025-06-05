using Frederikskaj2.Reservations.Calendar;
using Frederikskaj2.Reservations.Orders;
using Frederikskaj2.Reservations.Users;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Server.IntegrationTests.Harness;

static class ResidentOrderExtensions
{
    public static async ValueTask<GetReservedDaysResponse> GetMyReservedDays(this SessionFixture session) =>
        await session.Deserialize<GetReservedDaysResponse>(await session.Get("reserved-days/my"));

    public static async ValueTask<GetReservedDaysResponse> GetOwnerReservedDays(this SessionFixture session) =>
        await session.Deserialize<GetReservedDaysResponse>(await session.AdministratorGet("reserved-days/owner"));

    public static async ValueTask<GetMyOrderResponse> GetMyOrder(this SessionFixture session, OrderId orderId) =>
        await session.Deserialize<GetMyOrderResponse>(await session.Get($"orders/my/{orderId}"));

    public static async ValueTask<PlaceMyOrderResponse> PlaceResidentOrder(this SessionFixture session, params TestReservation[] reservations) =>
        await session.Deserialize<PlaceMyOrderResponse>(await session.PlaceResidentOrderRaw(reservations));

    public static async ValueTask<PlaceMyOrderResponse> PlaceResidentOrder(this SessionFixture session, params ReservationRequest[] reservations) =>
        await session.Deserialize<PlaceMyOrderResponse>(await session.PlaceResidentOrderRaw(reservations));

    public static ValueTask<HttpResponseMessage> PlaceResidentOrderRaw(this SessionFixture session, params TestReservation[] reservations) =>
        session.PlaceResidentOrderRaw(
            reservations
                .Select(reservation => new ReservationRequest(reservation.ResourceId, session.Calendar.GetAvailableExtent(session, reservation)))
                .ToArray());

    public static ValueTask<HttpResponseMessage> PlaceResidentOrderRaw(this SessionFixture session, params ReservationRequest[] reservations)
    {
        if (session.User is null)
            throw new InvalidOperationException();

        var request = new PlaceMyOrderRequest(session.User.FullName, session.User.Phone, session.User.ApartmentId, reservations, session.User.AccountNumber);
        return session.Post("orders/my", request);
    }

    public static async ValueTask<GetMyOrderResponse> PlaceAndPayResidentOrder(this SessionFixture session, params TestReservation[] reservations)
    {
        var response = await PlaceResidentOrder(session, reservations);
        await session.PayIn(response.Order.Payment!.PaymentId, response.Order.Price.Total());
        return await session.GetMyOrder(response.Order.OrderId);
    }

    public static async ValueTask<UpdateMyOrderResponse> CancelResidentReservations(
        this SessionFixture session, OrderId orderId, params ReservationIndex[] reservationIndices) =>
        await session.Deserialize<UpdateMyOrderResponse>(await session.UpdateResidentReservationRaw(orderId, reservationIndices));

    public static ValueTask<HttpResponseMessage> UpdateResidentReservationRaw(
        this SessionFixture session, OrderId orderId, params ReservationIndex[] reservationIndices)
    {
        if (session.User is null)
            throw new InvalidOperationException();

        var request = new UpdateMyOrderRequest(AccountNumber: null, reservationIndices.ToHashSet());
        return session.Patch($"orders/my/{orderId}", request);
    }

    public static async ValueTask<UpdateMyOrderResponse> CancelResidentReservationNoFee(
        this SessionFixture session, OrderId orderId, params ReservationIndex[] reservationIndices)
    {
        if (session.User is null)
            throw new InvalidOperationException();

        var request = new UpdateMyOrderRequest(AccountNumber: session.User.AccountNumber, CancelledReservations: reservationIndices.ToHashSet());
        return await session.Deserialize<UpdateMyOrderResponse>(await session.Patch($"orders/my/{orderId}", request));
    }

    public static async ValueTask<UpdateMyOrderResponse> UpdateResidentAccountNumber(this SessionFixture session, OrderId orderId, string accountNumber)
    {
        if (session.User is null)
            throw new InvalidOperationException();

        var request = new UpdateMyOrderRequest(accountNumber, CancelledReservations: null);
        return await session.Deserialize<UpdateMyOrderResponse>(await session.Patch($"orders/my/{orderId}", request));
    }

    public static ValueTask<HttpResponseMessage> UpdateResidentReservations(
        this SessionFixture session, OrderId orderId, params ReservationUpdateRequest[] reservations)
    {
        var request = new UpdateResidentReservationsRequest(Reservations: reservations);
        return session.AdministratorPatch($"orders/resident/{orderId}/reservations", request);
    }
}
