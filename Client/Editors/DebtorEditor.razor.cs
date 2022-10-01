using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Blazorise;
using Frederikskaj2.Reservations.Shared.Core;
using Microsoft.AspNetCore.Components;
using System.Linq;

namespace Frederikskaj2.Reservations.Client.Editors;

public partial class DebtorEditor
{
    const string noPaymentId = "-";

    IReadOnlyDictionary<string, Debtor>? debtorDictionary;
    Select<string> select = null!;
    string selectedPaymentId = noPaymentId;

    [Parameter] public IEnumerable<Debtor>? Debtors { get; set; }

    [Parameter] public Debtor? Value
    {
        get => debtorDictionary is not null && debtorDictionary.TryGetValue(selectedPaymentId, out var debtor) ? debtor : null;
        set => selectedPaymentId = value is not null ? value.PaymentId.ToString() : noPaymentId;
    }

    [Parameter] public EventCallback<Debtor?> ValueChanged { get; set; }

    public async ValueTask FocusAsync()
    {
        // https://github.com/dotnet/aspnetcore/issues/30070#issuecomment-823938686
        await Task.Yield();
        await select.ElementRef.FocusAsync();
    }

    protected override void OnParametersSet()
    {
        debtorDictionary = Debtors?.ToDictionary(debtor => debtor.PaymentId.ToString());
        selectedPaymentId = Value is not null ? Value.PaymentId.ToString() : noPaymentId;
    }

    Task SetPaymentIdAsync(string paymentId)
    {
        if (paymentId == selectedPaymentId)
            return Task.CompletedTask;
        selectedPaymentId = paymentId;
        return ValueChanged.InvokeAsync(Value);
    }

    static void Validate(ValidatorEventArgs e)
    {
        var paymentId = e.Value?.ToString();
        if (paymentId != noPaymentId)
        {
            e.Status = ValidationStatus.Success;
            return;
        }

        e.Status = ValidationStatus.Error;
        e.ErrorText = "Du skal angive en indbetaler";
    }
}
