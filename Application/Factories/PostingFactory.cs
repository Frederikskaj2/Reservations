using Frederikskaj2.Reservations.Shared.Core;
using LanguageExt;
using static LanguageExt.Prelude;

namespace Frederikskaj2.Reservations.Application;

static class PostingFactory
{
    public static Posting CreatePosting(HashMap<UserId, string> userNames, Transaction transaction) =>
        new(
            transaction.TransactionId,
            transaction.Date,
            transaction.Activity,
            transaction.UserId,
            userNames[transaction.UserId],
            transaction.OrderId,
            SafeGetAmounts(transaction));

    static Seq<AccountAmount> SafeGetAmounts(Transaction transaction) =>
        ValidateAmounts(transaction, GetAmounts(transaction));

    static Seq<AccountAmount> GetAmounts(Transaction transaction) =>
        Seq(
                GetIncome(transaction),
                GetBank(transaction),
                GetAccountsReceivable(transaction),
                GetDeposits(transaction),
                GetAccountsPayable(transaction))
            .Somes();

    static Seq<AccountAmount> ValidateAmounts(Transaction transaction, Seq<AccountAmount> amounts) =>
        !amounts.IsEmpty && amounts.Fold(Amount.Zero, (sum, amount) => sum + amount.Amount) == Amount.Zero
            ? amounts
            : throw new InternalValidationException($"Invalid {transaction}.");

    static Option<AccountAmount> GetIncome(Transaction transaction) =>
        GetAccountAmount(
            PostingAccount.Income,
            transaction.Amounts[Account.Rent] + transaction.Amounts[Account.Cleaning] + transaction.Amounts[Account.CancellationFees] + transaction.Amounts[Account.Damages]);

    static Option<AccountAmount> GetBank(Transaction transaction) =>
        GetAccountAmount(PostingAccount.Bank, transaction.Amounts[Account.Bank]);

    static Option<AccountAmount> GetAccountsReceivable(Transaction transaction) =>
        GetAccountAmount(PostingAccount.AccountsReceivable, transaction.Amounts[Account.AccountsReceivable]);

    static Option<AccountAmount> GetDeposits(Transaction transaction) =>
        GetAccountAmount(PostingAccount.Deposits, transaction.Amounts[Account.Deposits]);

    static Option<AccountAmount> GetAccountsPayable(Transaction transaction) =>
        GetAccountAmount(PostingAccount.AccountsPayable, transaction.Amounts[Account.AccountsPayable]);

    static Option<AccountAmount> GetAccountAmount(PostingAccount account, Amount amount) =>
        amount != Amount.Zero ? new AccountAmount(account, amount) : None;
}
