using Blazorise;
using Frederikskaj2.Reservations.Shared.Core;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Client.Shared;

public partial class OwnerOrderStatement
{
    HashSet<ReservationIndex> cancelledReservations = new();
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
    [Parameter] public IEnumerable<Reservation>? Reservations { get; set; }
    [Parameter] public bool IsCleaningRequired { get; set; }
    [Parameter] public EventCallback<(string? Description, HashSet<ReservationIndex> CancelledReservations, bool IsCleaningRequired)> OnSubmit { get; set; }

    bool IsSubmitDisabled =>
        string.IsNullOrWhiteSpace(description) || description == Description && cancelledReservations.Count is 0 && isCleaningRequired == IsCleaningRequired;

    protected override async Task OnInitializedAsync()
    {
        options = await ClientDataProvider.GetOptionsAsync();
        resources = await ClientDataProvider.GetResourcesAsync();
    }

    protected override void OnParametersSet()
    {
        description = Description;
        cancelledReservations = new();
        isCleaningRequired = IsCleaningRequired;
    }

    void ToggleCancelReservation(ReservationIndex reservationIndex)
    {
        if (!cancelledReservations.Contains(reservationIndex))
            cancelledReservations.Add(reservationIndex);
        else
            cancelledReservations.Remove(reservationIndex);
    }

    async Task SubmitAsync()
    {
        await validations.ClearAll();
        if (!await validations.ValidateAll())
            return;

        await OnSubmit.InvokeAsync((description, cancelledReservations, isCleaningRequired));
    }
}
