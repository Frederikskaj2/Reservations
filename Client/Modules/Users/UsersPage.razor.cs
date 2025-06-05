using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Client.Modules.Users;

[Authorize(Policy = Policy.ViewUsers)]
partial class UsersPage
{
    bool isInitialized;
    List<UserDto>? other;
    List<UserDto>? residents;
    List<UserDto>? superUsers;

    [Inject] public AuthenticatedApiClient ApiClient { get; set; } = null!;

    protected override async Task OnInitializedAsync()
    {
        var response = await ApiClient.Get<IEnumerable<UserDto>>("users");
        var users = response.IsSuccess ? response.Result! : [];
        var lookup = users.ToLookup(GetUserType);
        residents = GetUsers(lookup, UserType.Resident);
        superUsers = GetUsers(lookup, UserType.SuperUser);
        other = GetUsers(lookup, UserType.Other);
        isInitialized = true;
    }

    static List<UserDto> GetUsers(ILookup<UserType, UserDto> lookup, UserType type) =>
        lookup.Contains(type)
            ? lookup[type].OrderBy(user => user.Identity.ApartmentId?.ToInt32() ?? 999).ThenBy(user => user.Identity.FullName).ToList()
            : [];

    static UserType GetUserType(UserDto user) =>
        user.Roles switch
        {
            Roles.None => UserType.Other,
            Roles.Resident => UserType.Resident,
            _ => UserType.SuperUser,
        };

    enum UserType
    {
        Resident,
        SuperUser,
        Other,
    }
}
