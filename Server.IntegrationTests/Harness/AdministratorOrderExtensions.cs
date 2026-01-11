using Bogus;
using Frederikskaj2.Reservations.Orders;
using Frederikskaj2.Reservations.Users;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Server.IntegrationTests.Harness;

static class AdministratorOrderExtensions
{
    static readonly Faker dataFaker = new();

    public static ValueTask<PlaceOwnerOrderResponse> PlaceOwnerOrder(this SessionFixture session, params TestReservation[] reservations) =>
        session.PlaceOwnerOrder(isCleaningRequired: true, reservations);

    public static async ValueTask<PlaceOwnerOrderResponse> PlaceOwnerOrder(
        this SessionFixture session, bool isCleaningRequired, params TestReservation[] reservations)
    {
        var request = new PlaceOwnerOrderRequest(
            Description: dataFaker.Lorem.Sentence(),
            reservations.Select(reservation => new ReservationRequest(reservation.ResourceId, session.Calendar.GetAvailableExtent(session, reservation))),
            isCleaningRequired);
        return await session.Deserialize<PlaceOwnerOrderResponse>(await session.AdministratorPost("orders/owner", request));
    }

    public static async ValueTask<PlaceResidentOrderResponse> AdministratorPlaceResidentOrder(
        this SessionFixture session, params TestReservation[] reservations)
    {
        if (session.User is null)
            throw new InvalidOperationException();

        var request = new PlaceResidentOrderRequest(
            session.UserId(),
            session.User.FullName,
            session.User.Phone,
            session.User.ApartmentId,
            session.User.AccountNumber,
            reservations.Select(reservation => new ReservationRequest(reservation.ResourceId, session.Calendar.GetAvailableExtent(session, reservation))));
        return await session.Deserialize<PlaceResidentOrderResponse>(await session.AdministratorPost("orders/resident", request));
    }

    public static async ValueTask<UpdateOwnerOrderResponse> CancelOwnerReservation(
        this SessionFixture session, OrderId orderId, params ReservationIndex[] reservationIndices)
    {
        var request = new UpdateOwnerOrderRequest(Description: null, reservationIndices.ToHashSet(), IsCleaningRequired: null);
        return await session.Deserialize<UpdateOwnerOrderResponse>(await session.AdministratorPatch($"orders/owner/{orderId}", request));
    }

    public static async ValueTask<UpdateOwnerOrderResponse> UpdateOwnerOrderDescription(this SessionFixture session, OrderId orderId, string description)
    {
        var request = new UpdateOwnerOrderRequest(description, CancelledReservations: null, IsCleaningRequired: null);
        return await session.Deserialize<UpdateOwnerOrderResponse>(await session.AdministratorPatch($"orders/owner/{orderId}", request));
    }

    public static async ValueTask<UpdateOwnerOrderResponse> UpdateOwnerOrderCleaning(this SessionFixture session, OrderId orderId, bool isCleaningRequired)
    {
        var request = new UpdateOwnerOrderRequest(Description: null, CancelledReservations: null, isCleaningRequired);
        return await session.Deserialize<UpdateOwnerOrderResponse>(await session.AdministratorPatch($"orders/owner/{orderId}", request));
    }
}
