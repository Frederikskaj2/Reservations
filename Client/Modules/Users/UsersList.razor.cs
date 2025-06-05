using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Users;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Client.Modules.Users;

partial class UsersList
{
    IReadOnlyDictionary<ApartmentId, Apartment>? apartments;

    [Inject] public ClientDataProvider DataProvider { get; set; } = null!;
    [Inject] public Formatter Formatter { get; set; } = null!;

    [Parameter] public IEnumerable<UserDto>? Users { get; set; }

    protected override async Task OnInitializedAsync() =>
        apartments = await DataProvider.GetApartments();

    Apartment? GetApartment(ApartmentId? apartmentId) =>
        apartments?.TryGetValue(apartmentId ?? Apartment.Deleted.ApartmentId, out var apartment) ?? false ? apartment : null;

    static IEnumerable<string> GetRoleNames(Roles roles) =>
        Enum.GetValues<Roles>().Where(role => role is not Roles.None && roles.HasFlag(role)).Select(GetRoleName);

    static string GetRoleName(Roles role) => role switch
    {
        Roles.Resident => "Beboer",
        Roles.OrderHandling => "Bestillinger",
        Roles.Bookkeeping => "Bogføring",
        Roles.UserAdministration => "Brugeradministration",
        Roles.Cleaning => "Rengøring",
        Roles.LockBoxCodes => "Nøglebokskoder",
        _ => "?",
    };
}
