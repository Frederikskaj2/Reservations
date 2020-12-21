namespace Frederikskaj2.Reservations.Shared
{
    public enum Account
    {
        None,
        // Income
        Rent,
        Cleaning,
        CancellationFees,
        Damages,
        // Assets
        Bank,
        AccountsReceivable,
        FromPayments,
        // Liabilities
        Deposits,
        Payments,
        ToAccountsReceivable
    }
}
