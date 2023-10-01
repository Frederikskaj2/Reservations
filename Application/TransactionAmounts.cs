using Frederikskaj2.Reservations.Shared.Core;

namespace Frederikskaj2.Reservations.Application;

static class TransactionAmounts
{
    public static AccountAmounts PlaceOrder(Price price, Amount accountsPayableToSpend) =>
        AccountAmounts.Create(
            (Account.Rent, -price.Rent),
            (Account.Cleaning, -price.Cleaning),
            (Account.AccountsReceivable, price.Total() + accountsPayableToSpend),
            (Account.AccountsPayable, -accountsPayableToSpend),
            (Account.Deposits, -price.Deposit));

    public static AccountAmounts UpdateReservations(Price oldPrice, Price newPrice, Amount accountsPayableToSpend) =>
        AccountAmounts.Create(
            (Account.Rent, -(newPrice.Rent - oldPrice.Rent)),
            (Account.Cleaning, -(newPrice.Cleaning - oldPrice.Cleaning)),
            (Account.AccountsReceivable, newPrice.Total() - oldPrice.Total() + accountsPayableToSpend),
            (Account.AccountsPayable, -accountsPayableToSpend),
            (Account.Deposits, -(newPrice.Deposit - oldPrice.Deposit)));

    public static AccountAmounts CancelUnpaidReservation(Price price, Amount fee)
    {
        var amountToRefund = price.Total() - fee;
        return AccountAmounts.Create(
            (Account.Rent, price.Rent),
            (Account.Cleaning, price.Cleaning),
            (Account.CancellationFees, -fee),
            (Account.AccountsReceivable, -amountToRefund),
            (Account.Deposits, price.Deposit));
    }

    public static AccountAmounts CancelPaidReservation(Price price, Amount fee) =>
        CancelPaidReservation(price, fee, price.Total() - fee);

    static AccountAmounts CancelPaidReservation(Price price, Amount fee, Amount amountToRefund) =>
        AccountAmounts.Create(
            (Account.Rent, price.Rent),
            (Account.Cleaning, price.Cleaning),
            (Account.CancellationFees, -fee),
            (Account.AccountsPayable, -amountToRefund),
            (Account.Deposits, price.Deposit));

    public static AccountAmounts Settle(Price price, Amount damages) =>
        AccountAmounts.Create(
            (Account.Damages, -damages),
            (Account.Deposits, price.Deposit),
            (Account.AccountsPayable, -(price.Deposit - damages)));

    public static AccountAmounts PayIn(Amount amount, Amount excessAmount) =>
        AccountAmounts.Create(
            (Account.Bank, amount),
            (Account.AccountsReceivable, excessAmount - amount),
            (Account.AccountsPayable, -excessAmount));

    public static AccountAmounts PayOut(Amount amount, Amount excessAmount) =>
        AccountAmounts.Create(
            (Account.AccountsPayable, amount - excessAmount),
            (Account.Bank, -amount),
            (Account.AccountsReceivable, excessAmount));

    public static AccountAmounts Reimburse(Account incomeAccount, Amount amount) =>
        AccountAmounts.Create(
            (incomeAccount, amount),
            (Account.AccountsPayable, -amount));
}
