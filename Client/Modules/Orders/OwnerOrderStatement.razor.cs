using Blazorise;
using Frederikskaj2.Reservations.LockBox;
using Frederikskaj2.Reservations.Orders;
using Frederikskaj2.Reservations.Users;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Client.Modules.Orders;

partial class OwnerOrderStatement
{
    HashSet<ReservationIndex> cancelledReservations = [];
    string? description;
    bool isCleaningRequired;
    OrderingOptions? options;
    IReadOnlyDictionary<ResourceId, Resource>? resources;
    Validations validations = null!;

    [Inject] public ClientDataProvider ClientDataProvider { get; set; } = null!;
    [Inject] public Formatter Formatter { get; set; } = null!;

    [Parameter] public OrderId OrderId { get; set; }
    [Parameter] public bool IsHistoryOrder { get; set; }
    [Parameter] public string? Description { get; set; }
    [Parameter] public IEnumerable<ReservationDto>? Reservations { get; set; }
    [Parameter] public bool IsCleaningRequired { get; set; }
    [Parameter] public EventCallback<(string? Description, HashSet<ReservationIndex> CancelledReservations, bool IsCleaningRequired)> OnSubmit { get; set; }

    bool IsSubmitDisabled =>
        string.IsNullOrWhiteSpace(description) || description == Description && cancelledReservations.Count is 0 && isCleaningRequired == IsCleaningRequired;

    protected override async Task OnInitializedAsync()
    {
        options = await ClientDataProvider.GetOptions();
        resources = await ClientDataProvider.GetResources();
    }

    protected override void OnParametersSet()
    {
        description = Description;
        cancelledReservations = [];
        isCleaningRequired = IsCleaningRequired;
    }

    void ToggleCancelReservation(ReservationIndex reservationIndex)
    {
        if (!cancelledReservations.Add(reservationIndex))
            cancelledReservations.Remove(reservationIndex);
    }

    async Task Submit()
    {
        await validations.ClearAll();
        if (!await validations.ValidateAll())
            return;

        await OnSubmit.InvokeAsync((description, cancelledReservations, isCleaningRequired));
    }
}
