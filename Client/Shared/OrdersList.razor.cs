using Frederikskaj2.Reservations.Shared.Core;
using Microsoft.AspNetCore.Components;
using NodaTime;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Client.Shared;

public partial class OrdersList
{
    Dictionary<ApartmentId, Apartment>? apartments;
    OrderingOptions? options;

    [Inject] public ClientDataProvider ClientDataProvider { get; set; } = null!;
    [Inject] public IDateProvider DateProvider { get; set; } = null!;
    [Inject] public Formatter Formatter { get; set; } = null!;

    [Parameter] public IEnumerable<Order>? Orders { get; set; }

    protected override async Task OnInitializedAsync()
    {
        options = await ClientDataProvider.GetOptionsAsync();
        apartments = (await ClientDataProvider.GetApartmentsAsync())?.ToDictionary(apartment => apartment.ApartmentId);
    }

    static LocalDate GetNextReservationDate(Order order) =>
        order.Reservations.OrderBy(reservation => reservation.Extent.Date).First().Extent.Date;

    Apartment GetApartment(ApartmentId apartmentId) => apartments![apartmentId];
}
