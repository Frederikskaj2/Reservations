using Frederikskaj2.Reservations.LockBox;
using Frederikskaj2.Reservations.Orders;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Client.Modules.Orders;

partial class AdditionalLineItems
{
    IReadOnlyDictionary<ResourceId, Resource>? resources;

    [Inject] public ClientDataProvider DataProvider { get; set; } = null!;
    [Inject] public Formatter Formatter { get; set; } = null!;

    [Parameter] public IEnumerable<LineItem>? Items { get; set; }
    [Parameter] public IEnumerable<ReservationDto>? Reservations { get; set; }

    protected override async Task OnInitializedAsync() => resources = await DataProvider.GetResources();

    string GetReservationsDescription(CancellationFee cancellationFee)
    {
        var cancelledReservations = cancellationFee.Reservations.ToHashSet();
        var reservations = Reservations!
            .Select((reservation, index) => (Reservation: reservation, Index: index))
            .Where(tuple => cancelledReservations.Contains(tuple.Index))
            .Select(tuple => tuple.Reservation);
        return string.Join(", ", reservations.Select(GetReservationDescription));
    }

    string GetReservationDescription(ReservationIndex index) =>
        GetReservationDescription(Reservations!.ElementAt(index.ToInt32()));

    string GetReservationDescription(ReservationDto reservation) =>
        $"{resources![reservation.ResourceId].Name} {Formatter.FormatDate(reservation.Extent.Date)}";
}
