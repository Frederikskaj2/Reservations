using System;
using System.Threading;
using System.Threading.Tasks;
using Blazorise;
using Frederikskaj2.Reservations.Shared.Core;
using Frederikskaj2.Reservations.Shared.Web;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.Linq;

namespace Frederikskaj2.Reservations.Client.Shared;

public sealed partial class UserInformation : IDisposable
{
    string? address;
    IEnumerable<Apartment>? apartments;
    string? fullName;
    bool isDisabled;
    string? phone;
    Timer? timer;
    Validations validations = null!;

    [Inject] public ClientDataProvider ClientDataProvider { get; set; } = null!;
    [Inject] public IDateProvider DateProvider { get; set; } = null!;
    [Inject] public Formatter Formatter { get; set; } = null!;

    [Parameter] public EventCallback<UpdateUserRequest> OnUpdate { get; set; }
    [Parameter] public bool IsReadOnly { get; set; }
    [Parameter] public UserDetails User { get; set; } = null!;

    public void Dispose()
    {
        timer?.Dispose();
        timer = null;
    }

    protected override async Task OnInitializedAsync() => apartments = await ClientDataProvider.GetApartmentsAsync();

    protected override void OnParametersSet() => SetUser(User);

    protected override void OnAfterRender(bool firstRender)
    {
        if (firstRender)
            validations.ClearAll();
    }

    async Task UpdateUserAsync()
    {
        await validations.ClearAll();
        if (!await validations.ValidateAll())
            return;

        await validations.ClearAll();
        var request = new UpdateUserRequest
        {
            FullName = fullName,
            Phone = phone,
            Roles = User.Roles,
            IsPendingDelete = User.IsPendingDelete
        };
        await OnUpdate.InvokeAsync(request);
    }

    void SetUser(UserDetails user)
    {
        isDisabled = IsReadOnly || user.IsDeleted;
        fullName = user.Information.FullName!;
        address = user.Information.ApartmentId.HasValue && user.Information.ApartmentId != Apartment.Deleted.ApartmentId
            ? apartments!.First(apartment => apartment.ApartmentId == user.Information.ApartmentId.Value).ToString()
            : null;
        phone = user.Information.Phone!;
        if (user.LatestSignIn.HasValue && (DateProvider.Now - user.LatestSignIn.Value).TotalMinutes < 60)
            timer = new Timer(_ => OnTimerElapsed(), null, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));
    }

    void OnTimerElapsed() => StateHasChanged();
}
