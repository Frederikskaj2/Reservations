using Blazorise;
using Frederikskaj2.Reservations.LockBox;
using Frederikskaj2.Reservations.Orders;
using Frederikskaj2.Reservations.Users;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Client.Modules.Orders;

partial class SettlementDialog
{
    int damages;
    string? description;
    string? deposit;
    bool isConfirming;
    int maximumDamages;
    Modal modal = null!;
    string? reservationDate;
    ReservationIndex reservationIndex;
    string? reservationNights;
    string? resourceName;
    IReadOnlyDictionary<ResourceId, Resource>? resources;

    [Inject] public ClientDataProvider ClientDataProvider { get; set; } = null!;
    [Inject] public Formatter Formatter { get; set; } = null!;

    [Parameter] public EventCallback<(OrderId OrderId, SettleReservationRequest Request)> OnConfirm { get; set; }

    OrderId OrderId { get; set; }

    protected override async Task OnInitializedAsync() => resources = await ClientDataProvider.GetResources();

    public Task Show(OrderId orderId, ReservationDto reservation, ReservationIndex index)
    {
        OrderId = orderId;
        resourceName = resources![reservation.ResourceId].Name;
        reservationDate = Formatter.FormatDate(reservation.Extent.Date);
        reservationNights = reservation.Extent.Nights.ToString(CultureInfo.InvariantCulture);
        deposit = Formatter.FormatMoneyLong(reservation.Price!.Deposit);
        reservationIndex = index;
        damages = 0;
        maximumDamages = (int) reservation.Price!.Deposit.ToDecimal();
        isConfirming = false;
        return modal.Show();
    }

    Task Cancel() => modal.Hide();

    Task Confirm()
    {
        isConfirming = true;
        return modal.Hide();
    }

    Task OnModalClosed()
    {
        if (!isConfirming)
            return Task.CompletedTask;
        var request = new SettleReservationRequest(ReservationIndex: reservationIndex, Damages: Amount.FromInt32(damages), Description: description);
        return OnConfirm.InvokeAsync((OrderId, request));
    }
}
