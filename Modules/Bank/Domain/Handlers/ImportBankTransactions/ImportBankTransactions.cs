using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Users;
using LanguageExt;
using NodaTime;
using System.Diagnostics;
using System.Net;
using static LanguageExt.Prelude;

namespace Frederikskaj2.Reservations.Bank;

static class ImportBankTransactions
{
    public static Either<Failure<ImportError>, ImportBankTransactionsOutput> ImportBankTransactionsCore(ImportBankTransactionsInput input) =>
        from _1 in ValidateTransactionBalances(input.NewTransactions)
        from _2 in ValidateNotOld(input.NewTransactions, input.ExistingBankTransaction)
        from newTransactions in ValidateNoMissingTransactions(input.NewTransactions, input.ExistingBankTransaction)
        let latestImportStartDate = GetLatestImportStartDate(input.ExistingBankTransaction, newTransactions)
        select CreateOutput(input, newTransactions, latestImportStartDate);

    static Either<Failure<ImportError>, Unit> ValidateTransactionBalances(Seq<ImportBankTransaction> transactions) =>
        transactions.Length <= 1 ? unit : ValidateMultipleTransactionBalances(transactions).Map(_ => unit);

    static Either<Failure<ImportError>, Amount> ValidateMultipleTransactionBalances(Seq<ImportBankTransaction> transactions) =>
        transactions.Tail.FoldWhile(
            Either<Failure<ImportError>, Amount>.Right(transactions.Head.Balance),
            (either, transaction) => IsBalanceValid(either.Match(balance => balance, _ => throw new UnreachableException()), transaction)
                ? transaction.Balance
                : Failure.New(HttpStatusCode.UnprocessableEntity, ImportError.InvalidRequest, $"Balance of {transaction} is invalid."),
            either => either.Match(_ => true, _ => false));

    static bool IsBalanceValid(Amount previousBalance, ImportBankTransaction transaction) =>
        previousBalance + transaction.Amount == transaction.Balance;

    static Either<Failure<ImportError>, Unit> ValidateNotOld(Seq<ImportBankTransaction> newTransactions, Seq<BankTransaction> existingTransactions) =>
        newTransactions.IsEmpty ? unit : ValidateNotOld(newTransactions.Head, existingTransactions);

    static Either<Failure<ImportError>, Unit> ValidateNotOld(ImportBankTransaction firstNewTransaction, Seq<BankTransaction> existingTransactions) =>
        GetOldestTransactionDate(existingTransactions) <= firstNewTransaction.Date
            ? unit
            : Failure.New(HttpStatusCode.UnprocessableEntity, ImportError.OldTransactions, "Bank transactions are old.");

    static LocalDate GetOldestTransactionDate(Seq<BankTransaction> transactions) =>
        transactions.HeadOrNone().Case switch
        {
            BankTransaction transaction => transaction.Date,
            _ => LocalDate.MinIsoValue,
        };

    static Either<Failure<ImportError>, Seq<BankTransaction>> ValidateNoMissingTransactions(
        Seq<ImportBankTransaction> newTransactions, Seq<BankTransaction> existingTransactions) =>
        ValidateNoMissingTransactions(
            newTransactions,
            !existingTransactions.IsEmpty ? existingTransactions.Last : None,
            toHashSet(
                existingTransactions.Map(
                    transaction => new ImportBankTransaction(transaction.Date, transaction.Text, transaction.Amount, transaction.Balance))));

    static Either<Failure<ImportError>, Seq<BankTransaction>> ValidateNoMissingTransactions(
        Seq<ImportBankTransaction> newTransactions, Option<BankTransaction> latestTransactionOption, HashSet<ImportBankTransaction> existingTransactions) =>
        latestTransactionOption.Case switch
        {
            BankTransaction latestTransaction =>
                ValidateNoMissingTransactions(newTransactions.Filter(transaction => !existingTransactions.Contains(transaction)), latestTransaction),
            _ => CreateTransactions(newTransactions, BankTransactionId.FromInt32(1)),
        };

    static Either<Failure<ImportError>, Seq<BankTransaction>> ValidateNoMissingTransactions(
        Seq<ImportBankTransaction> newTransactions, BankTransaction latestTransaction) =>
        newTransactions.HeadOrNone().Case switch
        {
            ImportBankTransaction transaction => transaction.Date >= latestTransaction.Date && IsBalanceValid(latestTransaction.Balance, transaction)
                ? CreateTransactions(newTransactions, latestTransaction.BankTransactionId.GetNextId())
                : Failure.New(HttpStatusCode.UnprocessableEntity, ImportError.MissingTransactions, "Some transactions are missing."),
            _ => Seq<BankTransaction>(),
        };

    static Seq<BankTransaction> CreateTransactions(Seq<ImportBankTransaction> transactions, BankTransactionId nextId) =>
        transactions.Tail.Scan(
            CreateTransaction(nextId, transactions.Head),
            (previousTransaction, transaction) => CreateTransaction(previousTransaction.BankTransactionId.GetNextId(), transaction));

    static BankTransaction CreateTransaction(BankTransactionId id, ImportBankTransaction transaction) =>
        new(id, transaction.Date, transaction.Text, transaction.Amount, transaction.Balance, GetInitialStatus(transaction));

    static BankTransactionStatus GetInitialStatus(ImportBankTransaction transaction) =>
        IsPossiblePayment(transaction) ? BankTransactionStatus.Unknown : BankTransactionStatus.Ignored;

    static bool IsPossiblePayment(ImportBankTransaction transaction) =>
        PaymentIdMatcher.HasPaymentId(transaction.Text);

    static Option<LocalDate> GetLatestImportStartDate(Seq<BankTransaction> existingTransactions, Seq<BankTransaction> newTransactions) =>
        newTransactions.IsEmpty
            ? existingTransactions.HeadOrNone().Map(transaction => transaction.Date)
            : newTransactions.HeadOrNone().Map(transaction => transaction.Date);

    static ImportBankTransactionsOutput CreateOutput(
        ImportBankTransactionsInput input, Seq<BankTransaction> newTransactions, Option<LocalDate> latestImportStartDate) =>
        new(
            newTransactions,
            GetDateRange(input.ExistingBankTransaction, newTransactions),
            latestImportStartDate,
            GetImport(input.Command.Timestamp, newTransactions));

    static Option<DateRange> GetDateRange(Seq<BankTransaction> existingTransactions, Seq<BankTransaction> newTransactions) =>
        (existingTransactions.IsEmpty, newTransactions.IsEmpty) switch
        {
            (false, false) => new DateRange(existingTransactions[0].Date, newTransactions[^1].Date),
            (true, false) => new DateRange(newTransactions[0].Date, newTransactions[^1].Date),
            (false, true) => new DateRange(existingTransactions[0].Date, existingTransactions[^1].Date),
            _ => None,
        };

    static Option<Import> GetImport(Instant timestamp, Seq<BankTransaction> transactions) =>
        transactions.HeadOrNone().Case switch
        {
            BankTransaction transaction => new Import(timestamp, transaction.Date),
            _ => None,
        };
}
