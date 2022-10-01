using Blazorise;
using Frederikskaj2.Reservations.Shared.Core;
using Microsoft.AspNetCore.Components;
using System;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Client.Dialogs;

public partial class ReservationDialog
{
    string deposit = "";
    bool isConfirming;
    Modal modal = null!;
    string rent = "";
    Select<int> select = null!;
    string total = "";

    [Inject] public ClientDataProvider DataProvider { get; set; } = null!;
    [Inject] public Formatter Formatter { get; set; } = null!;

    [Parameter] public EventCallback OnCancel { get; set; }
    [Parameter] public EventCallback OnConfirm { get; set; }
    [Parameter] public DraftOrder? Order { get; set; }
    [Parameter] public bool ShowPrices { get; set; }

    public OrderingOptions? Options { get; set; }
    public int MinimumNights { get; set; } = 1;
    public int MaximumNights { get; set; } = 7;
    public bool ShowWarning { get; set; }

    bool IsSubmitDisabled => (Order?.DraftReservation?.Extent.Nights ?? 0) is 0;

    public void Show()
    {
        isConfirming = false;
        modal.Show();
    }

    protected override void OnParametersSet() => UpdatePrice();

    protected override async Task OnAfterRenderAsync(bool firstRender) => await select.ElementRef.FocusAsync();

    void Cancel() => modal.Hide();

    void Confirm()
    {
        isConfirming = true;
        modal.Hide();
    }

    Task OnModalClosed() => isConfirming ? OnConfirm.InvokeAsync(null) : OnCancel.InvokeAsync(null);

    void NightsChanged(int value)
    {
        Order?.UpdateReservation(new Extent(Order.DraftReservation!.Extent.Date, value));
        UpdatePrice();
    }

    void UpdatePrice()
    {
        if ((Order?.DraftReservation?.Extent.Nights ?? 0) is 0)
        {
            rent = "";
            deposit = "";
            total = "";
            return;
        }

        var price = Pricing.GetPrice(Options!, DataProvider.Holidays, Order!.DraftReservation!.Extent, Order.DraftReservation.Resource.Type);
        var rentAndCleaning = Formatter.FormatMoneyLong(price.Rent + price.Cleaning);
        var cleaning = Formatter.FormatMoneyLong(price.Cleaning);
        rent = $"{rentAndCleaning} (inklusiv rengøring {cleaning})";
        deposit = Formatter.FormatMoneyLong(price.Deposit);
        total = Formatter.FormatMoneyLong(price.Total());
    }

    string GetCheckInTime() => Order?.DraftReservation is not null
        ? Formatter.FormatCheckInTimeLong(Options!, Order.DraftReservation.Extent.Date)
        : "";

    string GetCheckOutTime() => (Order?.DraftReservation?.Extent.Nights ?? 0) > 0
        ? Formatter.FormatCheckOutTimeLong(Options!, Order!.DraftReservation!.Extent.Ends())
        : "";
}
