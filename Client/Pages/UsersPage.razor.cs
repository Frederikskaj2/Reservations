using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Frederikskaj2.Reservations.Shared.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;

namespace Frederikskaj2.Reservations.Client.Pages;

[Authorize(Policy = Policies.ViewUsers)]
public partial class UsersPage
{
    bool isInitialized;
    List<User>? other;
    List<User>? residents;
    List<User>? superUsers;

    [Inject] public AuthenticatedApiClient ApiClient { get; set; } = null!;

    protected override async Task OnInitializedAsync()
    {
        var response = await ApiClient.GetAsync<IEnumerable<User>>("users");
        var users = response.IsSuccess ? response.Result! : Enumerable.Empty<User>();
        var lookup = users.ToLookup(GetUserType);
        residents = GetUsers(lookup, UserType.Resident);
        superUsers = GetUsers(lookup, UserType.SuperUser);
        other = GetUsers(lookup, UserType.Other);
        isInitialized = true;
    }

    static List<User> GetUsers(ILookup<UserType, User> lookup, UserType type) =>
        lookup.Contains(type)
            ? lookup[type].OrderBy(user => user.Information.ApartmentId?.ToInt32() ?? 999).ThenBy(user => user.Information.FullName).ToList()
            : new List<User>();

    static UserType GetUserType(User user) =>
        user.Roles switch
        {
            Roles.None => UserType.Other,
            Roles.Resident => UserType.Resident,
            _ => UserType.SuperUser
        };

    enum UserType
    {
        Resident,
        SuperUser,
        Other
    }
}
