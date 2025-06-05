using Frederikskaj2.Reservations.Users;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Client.Modules.Bank;

partial class Counterparty
{
    IReadOnlyDictionary<ApartmentId, Apartment>? apartments;

    [Inject] public ClientDataProvider ClientDataProvider { get; set; } = null!;
    [Inject] public CultureInfo CultureInfo { get; set; } = null!;

    [Parameter] public PaymentId PaymentId { get; set; }
    [Parameter] public Amount Amount { get; set; }
    [Parameter] public UserIdentityDto? User { get; set; }

    protected override async Task OnInitializedAsync() => apartments = await ClientDataProvider.GetApartments();
}
