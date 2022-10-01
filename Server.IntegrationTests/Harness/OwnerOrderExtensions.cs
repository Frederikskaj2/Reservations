using Bogus;
using Frederikskaj2.Reservations.Shared.Core;
using Frederikskaj2.Reservations.Shared.Web;
using System.Linq;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Server.IntegrationTests.Harness;

static class OwnerOrderExtensions
{
    static readonly Faker dataFaker = new();

    public static ValueTask<OrderDetails> PlaceOwnerOrderAsync(this SessionFixture session, params TestReservation[] reservations) =>
        session.PlaceOwnerOrderAsync(true, reservations);

    public static async ValueTask<OrderDetails> PlaceOwnerOrderAsync(this SessionFixture session, bool isCleaningRequired, params TestReservation[] reservations)
    {
        var request = new PlaceOwnerOrderRequest
        {
            Description = dataFaker.Lorem.Sentence(),
            Reservations = reservations.Select(reservation => new ReservationRequest
            {
                ResourceId = reservation.ResourceId,
                Extent = session.Calendar.GetAvailableExtent(session, reservation)
            }),
            IsCleaningRequired = isCleaningRequired
        };
        return await session.DeserializeAsync<OrderDetails>(await session.AdministratorPostAsync("orders/owner", request));
    }

    public static async ValueTask<OrderDetails> CancelOwnerReservationAsync(
        this SessionFixture session, OrderId orderId, params ReservationIndex[] reservationIndices)
    {
        var request = new UpdateOwnerOrderRequest
        {
            CancelledReservations = reservationIndices.ToHashSet()
        };
        return await session.DeserializeAsync<OrderDetails>(await session.AdministratorPatchAsync($"orders/owner/{orderId}", request));
    }

    public static async ValueTask<OrderDetails> UpdateOwnerOrderDescriptionAsync(this SessionFixture session, OrderId orderId, string description)
    {
        var request = new UpdateOwnerOrderRequest
        {
            Description = description
        };
        return await session.DeserializeAsync<OrderDetails>(await session.AdministratorPatchAsync($"orders/owner/{orderId}", request));
    }

    public static async ValueTask<OrderDetails> UpdateOwnerOrderDescriptionAndCancelReservationsAsync(
        this SessionFixture session, OrderId orderId, string description, params ReservationIndex[] reservationIndices)
    {
        var request = new UpdateOwnerOrderRequest
        {
            Description = description,
            CancelledReservations = reservationIndices.ToHashSet()
        };
        return await session.DeserializeAsync<OrderDetails>(await session.AdministratorPatchAsync($"orders/owner/{orderId}", request));
    }

    public static async ValueTask<OrderDetails> UpdateOwnerOrderCleaningAsync(this SessionFixture session, OrderId orderId, bool isCleaningRequired)
    {
        var request = new UpdateOwnerOrderRequest
        {
            IsCleaningRequired = isCleaningRequired
        };
        return await session.DeserializeAsync<OrderDetails>(await session.AdministratorPatchAsync($"orders/owner/{orderId}", request));
    }
}
