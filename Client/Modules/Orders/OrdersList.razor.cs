using Frederikskaj2.Reservations.Orders;
using Frederikskaj2.Reservations.Users;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Client.Modules.Orders;

partial class OrdersList
{
    IReadOnlyDictionary<ApartmentId, Apartment>? apartments;
    bool isInitialized;

    [Inject] public ClientDataProvider ClientDataProvider { get; set; } = null!;
    [Inject] public Formatter Formatter { get; set; } = null!;

    [Parameter] public IEnumerable<OrderSummaryDto>? Orders { get; set; }

    protected override async Task OnInitializedAsync()
    {
        apartments = await ClientDataProvider.GetApartments();
        isInitialized = true;
    }
}
