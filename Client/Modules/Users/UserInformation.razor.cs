using Blazorise;
using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Users;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Client.Modules.Users;

public sealed partial class UserInformation : IDisposable
{
    string? address;
    IReadOnlyDictionary<ApartmentId, Apartment>? apartments;
    string? fullName;
    bool isDisabled;
    string? phone;
    Timer? timer;
    Validations validations = null!;

    [Inject] public ClientDataProvider ClientDataProvider { get; set; } = null!;
    [Inject] public IDateProvider DateProvider { get; set; } = null!;
    [Inject] public Formatter Formatter { get; set; } = null!;
    [Inject] public ITimeConverter TimeConverter { get; set; } = null!;

    [Parameter] public EventCallback<UpdateUserRequest> OnUpdate { get; set; }
    [Parameter] public bool IsReadOnly { get; set; }
    [Parameter] public UserDetailsDto User { get; set; } = null!;

    public void Dispose()
    {
        timer?.Dispose();
        timer = null;
    }

    protected override async Task OnInitializedAsync() => apartments = await ClientDataProvider.GetApartments();

    protected override void OnParametersSet() => SetUser(User);

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
            await validations.ClearAll();
    }

    async Task UpdateUser()
    {
        await validations.ClearAll();
        if (!await validations.ValidateAll())
            return;

        await validations.ClearAll();
        var request = new UpdateUserRequest(fullName, phone, User.Roles, User.IsPendingDelete);
        await OnUpdate.InvokeAsync(request);
    }

    void SetUser(UserDetailsDto user)
    {
        isDisabled = IsReadOnly || user.IsDeleted;
        fullName = user.Identity.FullName;
        address = user.Identity.ApartmentId.HasValue && user.Identity.ApartmentId != Apartment.Deleted.ApartmentId
            ? apartments![user.Identity.ApartmentId.Value].ToString()
            : null;
        phone = user.Identity.Phone;
        if (user.LatestSignIn.HasValue && (DateProvider.Now - user.LatestSignIn.Value).TotalMinutes < 60)
            timer = new(_ => OnTimerElapsed(), state: null, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));
    }

    void OnTimerElapsed() => StateHasChanged();
}
