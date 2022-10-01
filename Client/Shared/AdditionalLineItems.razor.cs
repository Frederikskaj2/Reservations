using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Frederikskaj2.Reservations.Shared.Core;
using Microsoft.AspNetCore.Components;
using System;

namespace Frederikskaj2.Reservations.Client.Shared;

public partial class AdditionalLineItems
{
    IReadOnlyDictionary<ResourceId, Resource>? resources;

    [Inject] public ClientDataProvider DataProvider { get; set; } = null!;
    [Inject] public Formatter Formatter { get; set; } = null!;

    [Parameter] public IEnumerable<LineItem>? Items { get; set; }
    [Parameter] public IEnumerable<Reservation>? Reservations { get; set; }

    protected override async Task OnInitializedAsync() => resources = await DataProvider.GetResourcesAsync();

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

    string GetReservationDescription(Reservation reservation) =>
        $"{resources![reservation.ResourceId].Name} {Formatter.FormatDate(reservation.Extent.Date)}";
}
