using Frederikskaj2.Reservations.Shared.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Client.Pages;

[Authorize(Policy = Policies.ViewOrders)]
public partial class OrdersPage
{
    List<Order>? confirmedOrders;
    bool isInitialized;
    OrderingOptions? options;
    List<Order>? reservedOrders;
    List<Order>? unsettledOrders;

    [Inject] public AuthenticatedApiClient ApiClient { get; set; } = null!;
    [Inject] public ClientDataProvider ClientDataProvider { get; set; } = null!;
    [Inject] public IDateProvider DateProvider { get; set; } = null!;
    [Inject] public Formatter Formatter { get; set; } = null!;

    protected override async Task OnInitializedAsync()
    {
        options = await ClientDataProvider.GetOptionsAsync();

        var orders = await GetOrders();
        var lookup = orders.ToLookup(GetOrderStatus);
        reservedOrders = lookup.Contains(OrderStatus.Reserved) ? lookup[OrderStatus.Reserved].ToList() : new List<Order>();
        confirmedOrders = lookup.Contains(OrderStatus.Confirmed) ? lookup[OrderStatus.Confirmed].ToList() : new List<Order>();
        unsettledOrders = lookup.Contains(OrderStatus.NeedsSettlement) ? lookup[OrderStatus.NeedsSettlement].ToList() : new List<Order>();

        isInitialized = true;

        async ValueTask<IEnumerable<Order>> GetOrders()
        {
            var response = await ApiClient.GetAsync<IEnumerable<Order>>("orders/user");
            return response.IsSuccess ? response.Result! : Enumerable.Empty<Order>();
        }

        OrderStatus GetOrderStatus(Order order) =>
            order.Reservations.Any(reservation => reservation.Status is ReservationStatus.Reserved)
                ? OrderStatus.Reserved
                : GetReservationsToSettle(order).Any()
                    ? OrderStatus.NeedsSettlement
                    : OrderStatus.Confirmed;

        IEnumerable<Reservation> GetReservationsToSettle(Order order) =>
            order.Reservations.Where(reservation =>
                reservation.Status is ReservationStatus.Confirmed &&
                (reservation.Extent.Ends() <= DateProvider.Today || (options!.Testing?.IsSettlementAlwaysAllowed ?? false)));
    }

    enum OrderStatus
    {
        Reserved,
        Confirmed,
        NeedsSettlement
    }
}
