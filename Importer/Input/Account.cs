namespace Frederikskaj2.Reservations.Importer.Input;

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