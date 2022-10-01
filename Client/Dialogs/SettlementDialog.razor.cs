using Blazorise;
using Frederikskaj2.Reservations.Shared.Core;
using Frederikskaj2.Reservations.Shared.Web;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Client.Dialogs;

public partial class SettlementDialog
{
    readonly SettleReservationRequest request = new();
    int damages;
    string? deposit;
    bool isConfirming;
    int maximumDamages;
    Modal modal = null!;
    OrderId orderId;
    string? reservationDate;
    string? reservationNights;
    string? resourceName;
    IReadOnlyDictionary<ResourceId, Resource>? resources;
    Validations validations = null!;

    [Inject] public ClientDataProvider ClientDataProvider { get; set; } = null!;
    [Inject] public Formatter Formatter { get; set; } = null!;

    [Parameter] public EventCallback<(OrderId OrderId, SettleReservationRequest Request)> OnConfirm { get; set; }
    [Parameter] public EventCallback OnCancel { get; set; }

    protected override async Task OnInitializedAsync() => resources = await ClientDataProvider.GetResourcesAsync();

    public void Show(UserId userId, OrderId orderId, Reservation reservation, ReservationIndex index)
    {
        this.orderId = orderId;
        resourceName = resources![reservation.ResourceId].Name;
        reservationDate = Formatter.FormatDate(reservation.Extent.Date);
        reservationNights = reservation.Extent.Nights.ToString(CultureInfo.InvariantCulture);
        deposit = Formatter.FormatMoneyLong(reservation.Price!.Deposit);
        request.UserId = userId;
        request.ReservationId = new(orderId, index);
        damages = 0;
        maximumDamages = reservation.Price!.Deposit.ToInt32();
        isConfirming = false;
        modal.Show();
    }

    void ValidateDamages(ValidatorEventArgs e)
    {
        var damages = (int) e.Value;
        if (0 <= damages && damages <= maximumDamages)
            e.Status = ValidationStatus.Success;
        else
        {
            e.Status = ValidationStatus.Error;
            e.ErrorText = $"Angiv et beløb mellem 0 og {maximumDamages}.";
        }
    }

    void ValidateDescription(ValidatorEventArgs e)
    {
        var description = (string) e.Value;
        if (damages == 0)
            return;
        if (string.IsNullOrWhiteSpace(description))
        {
            e.Status = ValidationStatus.Error;
            e.ErrorText = "Angiv en beskrivelse af skaderne.";
        }
        else if (description.Length > ValidationRules.MaximumDamagesDescriptionLength)
        {
            e.Status = ValidationStatus.Error;
            e.ErrorText = "Beskrivelsen er for lang.";
        }
    }

    void Cancel() => modal.Hide();

    async Task ConfirmAsync()
    {
        await validations.ClearAll();
        if (!await validations.ValidateAll())
            return;

        await validations.ClearAll();
        request.Damages = Amount.FromInt32(damages);
        isConfirming = true;
        await modal.Hide();
    }

    Task OnModalClosed() => isConfirming ? OnConfirm.InvokeAsync((orderId, request)) : OnCancel.InvokeAsync(null);
}
