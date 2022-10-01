namespace Frederikskaj2.Reservations.Application;

public enum Account
{
    None,
    // Income
    Rent = 10,
    Cleaning,
    CancellationFees,
    Damages,
    // Assets
    Bank = 30,
    AccountsReceivable,
    // Liabilities
    Deposits = 40,
    AccountsPayable
}