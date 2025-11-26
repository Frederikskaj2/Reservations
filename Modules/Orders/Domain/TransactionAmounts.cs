using Frederikskaj2.Reservations.Users;
using System;

namespace Frederikskaj2.Reservations.Orders;

public static class TransactionAmounts
{
    public static AccountAmounts PlaceOrder(Price price, Amount accountsPayableToSpend)
    {
        if (price.Total() <= Amount.Zero)
            throw new ArgumentOutOfRangeException(nameof(price), price, "Price must be greater than zero.");
        if (accountsPayableToSpend > Amount.Zero)
            throw new ArgumentOutOfRangeException(nameof(accountsPayableToSpend), accountsPayableToSpend, "Accounts payable to spend must not be positive.");
        return AccountAmounts.Create(
            (Account.Rent, -price.Rent),
            (Account.Cleaning, -price.Cleaning),
            (Account.AccountsReceivable, price.Total() + accountsPayableToSpend),
            (Account.AccountsPayable, -accountsPayableToSpend),
            (Account.Deposits, -price.Deposit));
    }

    public static AccountAmounts UpdateReservations(Price oldPrice, Price newPrice, Amount accountsPayableToSpend)
    {
        if (oldPrice.Total() <= Amount.Zero)
            throw new ArgumentOutOfRangeException(nameof(oldPrice), oldPrice, "The old price must be greater than zero.");
        if (newPrice.Total() <= Amount.Zero)
            throw new ArgumentOutOfRangeException(nameof(newPrice), newPrice, "The new price must be greater than zero.");
        if (accountsPayableToSpend > Amount.Zero)
            throw new ArgumentOutOfRangeException(nameof(accountsPayableToSpend), accountsPayableToSpend, "Accounts payable to spend must not be positive.");
        return AccountAmounts.Create(
            (Account.Rent, -(newPrice.Rent - oldPrice.Rent)),
            (Account.Cleaning, -(newPrice.Cleaning - oldPrice.Cleaning)),
            (Account.AccountsReceivable, newPrice.Total() - oldPrice.Total() + accountsPayableToSpend),
            (Account.AccountsPayable, -accountsPayableToSpend),
            (Account.Deposits, -(newPrice.Deposit - oldPrice.Deposit)));
    }

    public static AccountAmounts CancelPaidReservation(Price price, Amount fee)
    {
        if (price.Total() <= Amount.Zero)
            throw new ArgumentOutOfRangeException(nameof(price), price, "Price must be greater than zero.");
        if (fee < Amount.Zero)
            throw new ArgumentOutOfRangeException(nameof(fee), fee, "Fee must not be negative.");
        return CancelPaidReservation(price, fee, price.Total() - fee);
    }

    static AccountAmounts CancelPaidReservation(Price price, Amount fee, Amount amountToRefund)
    {
        if (price.Total() <= Amount.Zero)
            throw new ArgumentOutOfRangeException(nameof(price), price, "Price must be greater than zero.");
        if (fee < Amount.Zero)
            throw new ArgumentOutOfRangeException(nameof(fee), fee, "Fee must not be negative.");
        if (amountToRefund < Amount.Zero)
            throw new ArgumentOutOfRangeException(nameof(amountToRefund), amountToRefund, "Amount to refund must not be negative.");
        return AccountAmounts.Create(
            (Account.Rent, price.Rent),
            (Account.Cleaning, price.Cleaning),
            (Account.CancellationFees, -fee),
            (Account.AccountsPayable, -amountToRefund),
            (Account.Deposits, price.Deposit));
    }

    public static AccountAmounts CancelUnpaidReservation(Price price, Amount fee)
    {
        if (price.Total() <= Amount.Zero)
            throw new ArgumentOutOfRangeException(nameof(price), price, "Price must be greater than zero.");
        if (fee < Amount.Zero)
            throw new ArgumentOutOfRangeException(nameof(fee), fee, "Fee must not be negative.");
        var amountToRefund = price.Total() - fee;
        return AccountAmounts.Create(
            (Account.Rent, price.Rent),
            (Account.Cleaning, price.Cleaning),
            (Account.CancellationFees, -fee),
            (Account.AccountsReceivable, -amountToRefund),
            (Account.Deposits, price.Deposit));
    }

    public static AccountAmounts Settle(Price price, Amount damages)
    {
        if (damages < Amount.Zero)
            throw new ArgumentOutOfRangeException(nameof(damages), damages, "Damages must not be negative.");
        return AccountAmounts.Create(
            (Account.Damages, -damages),
            (Account.Deposits, price.Deposit),
            (Account.AccountsPayable, -(price.Deposit - damages)));
    }

    public static AccountAmounts PayIn(Amount amount, Amount excessAmount)
    {
        if (amount <= Amount.Zero)
            throw new ArgumentOutOfRangeException(nameof(amount), amount, "Amount must be greater than zero.");
        if (excessAmount < Amount.Zero)
            throw new ArgumentOutOfRangeException(nameof(excessAmount), excessAmount, "Excess amount must not be negative.");
        return AccountAmounts.Create(
            (Account.Bank, amount),
            (Account.AccountsReceivable, excessAmount - amount),
            (Account.AccountsPayable, -excessAmount));
    }

    public static AccountAmounts PayOut(Amount amount, Amount excessAmount)
    {
        if (amount <= Amount.Zero)
            throw new ArgumentOutOfRangeException(nameof(amount), amount, "Amount must be greater than zero.");
        if (excessAmount < Amount.Zero)
            throw new ArgumentOutOfRangeException(nameof(excessAmount), excessAmount, "Excess amount must not be negative.");
        return AccountAmounts.Create(
            (Account.AccountsPayable, amount - excessAmount),
            (Account.Bank, -amount),
            (Account.AccountsReceivable, excessAmount));
    }

    public static AccountAmounts Reimburse(Account incomeAccount, Amount amount, Amount accountsPayable, Amount accountsReceivable)
    {
        if (amount <= Amount.Zero)
            throw new ArgumentOutOfRangeException(nameof(amount), amount, "Amount must be greater than zero.");
        if (accountsPayable < Amount.Zero)
            throw new ArgumentOutOfRangeException(nameof(amount), amount, "Accounts payable must not be negative.");
        if (accountsReceivable < Amount.Zero)
            throw new ArgumentOutOfRangeException(nameof(amount), amount, "Accounts receivable must not be negative.");
        if (amount != accountsPayable + accountsReceivable)
            throw new ArgumentException("The sum of accounts payable and accounts receivable must be equal to the amount to reimburse.", nameof(amount));
        return AccountAmounts.Create(
            (incomeAccount, amount),
            (Account.AccountsReceivable, -accountsReceivable),
            (Account.AccountsPayable, -accountsPayable));
    }
}
