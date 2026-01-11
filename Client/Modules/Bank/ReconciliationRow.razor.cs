using Frederikskaj2.Reservations.Bank;
using Frederikskaj2.Reservations.Orders;
using Frederikskaj2.Reservations.Users;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Client.Modules.Bank;

partial class ReconciliationRow
{
    List<PayOutSummaryDto> otherPayOuts = [];
    ResidentDto? suggestedResident;
    PayOutSummaryDto? suggestedPayOut;

    [Inject] public CultureInfo CultureInfo { get; set; } = null!;
    [Inject] public Formatter Formatter { get; set; } = null!;

    [Parameter] public ConfirmReconciliationDialog? ConfirmReconciliationDialog { get; set; }
    [Parameter] public IEnumerable<ResidentDto>? Residents { get; set; }
    [Parameter] public FindResidentDialog? FindResidentDialog { get; set; }
    [Parameter] public EventCallback<(BankTransactionId, BankTransactionStatus)> OnSetTransactionStatus { get; set; }
    [Parameter] public IEnumerable<PayOutSummaryDto>? PayOuts { get; set; }
    [Parameter] public BankTransactionDto? Transaction { get; set; }

    protected override void OnParametersSet()
    {
        suggestedResident = null;
        suggestedPayOut = null;
        otherPayOuts = [];
        if (Transaction is null || Residents is null || PayOuts is null)
            return;
        if (!PaymentIdMatcher.TryGetPaymentId(Transaction.Text, out var paymentId))
            return;
        if (Transaction.Amount > Amount.Zero)
            suggestedResident = Residents.FirstOrDefault(debtor => debtor.PaymentId == paymentId);
        else
        {
            suggestedPayOut = PayOuts.FirstOrDefault(payOut =>
                payOut.PaymentId == paymentId &&
                payOut.Status is Reservations.Bank.PayOutStatus.InProgress &&
                payOut.Amount == -Transaction.Amount);
            otherPayOuts = PayOuts.Where(payOut =>
                payOut.PaymentId != suggestedPayOut?.PaymentId &&
                payOut.Status is Reservations.Bank.PayOutStatus.InProgress &&
                payOut.Amount == -Transaction.Amount).ToList();
        }
    }

    Task ReconcilePayIn(ResidentDto resident) => ConfirmReconciliationDialog!.Show(Transaction!, resident);

    Task FindResident() => FindResidentDialog!.Show(Transaction!);

    Task ReconcilePayOut(PayOutSummaryDto payOut)
    {
        var resident = Residents!.First(resident => resident.UserIdentity.UserId == payOut.UserIdentity.UserId);
        return ConfirmReconciliationDialog!.Show(Transaction!, resident, payOut);
    }

    Task SetTransactionStatus(BankTransactionStatus status) => OnSetTransactionStatus.InvokeAsync((Transaction!.BankTransactionId, status));
}
