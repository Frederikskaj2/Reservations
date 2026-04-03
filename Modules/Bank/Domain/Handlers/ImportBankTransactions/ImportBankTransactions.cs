using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Users;
using LanguageExt;
using NodaTime;
using System;
using System.Diagnostics;
using System.Net;
using static LanguageExt.Prelude;

namespace Frederikskaj2.Reservations.Bank;

static class ImportBankTransactions
{
    public static Either<Failure<ImportError>, ImportBankTransactionsOutput> ImportBankTransactionsCore(ImportBankTransactionsInput input) =>
        from _1 in ValidateTransactionBalances(input.NewTransactions)
        let existingBankTransactionsForAccount = input.ExistingBankTransaction.Filter(transaction => transaction.BankAccountId == input.Command.BankAccountId)
        from _2 in ValidateNotOld(input.NewTransactions, existingBankTransactionsForAccount)
        let nextBankTransactionId = GetNextBankTransactionId(input.ExistingBankTransaction)
        from newTransactions in ValidateNoMissingTransactions(
            input.Command.BankAccountId, input.NewTransactions, existingBankTransactionsForAccount, nextBankTransactionId)
        let latestImportStartDate = GetLatestImportStartDate(existingBankTransactionsForAccount, newTransactions)
        select CreateOutput(input, existingBankTransactionsForAccount, newTransactions, latestImportStartDate);

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

    static BankTransactionId GetNextBankTransactionId(Seq<BankTransaction> existingBankTransactions) =>
        existingBankTransactions.IsEmpty ? BankTransactionId.FromInt32(1) : existingBankTransactions.Last.BankTransactionId.GetNextId();

    static Either<Failure<ImportError>, Seq<BankTransaction>> ValidateNoMissingTransactions(
        BankAccountId bankAccountId,
        Seq<ImportBankTransaction> newTransactions,
        Seq<BankTransaction> existingTransactionsForAccount,
        BankTransactionId nextBankTransactionId) =>
        ValidateNoMissingTransactions(
            bankAccountId,
            newTransactions,
            nextBankTransactionId,
            !existingTransactionsForAccount.IsEmpty ? existingTransactionsForAccount.Last : None,
            toHashSet(
                existingTransactionsForAccount
                    .Map(transaction => new CompareBankTransaction(transaction.Date, transaction.Text, transaction.Amount, transaction.Balance))));

    static Either<Failure<ImportError>, Seq<BankTransaction>> ValidateNoMissingTransactions(
        BankAccountId bankAccountId,
        Seq<ImportBankTransaction> newTransactions,
        BankTransactionId nextBankTransactionId,
        Option<BankTransaction> latestTransactionOption,
        HashSet<CompareBankTransaction> existingTransactions) =>
        latestTransactionOption.Case switch
        {
            BankTransaction latestTransaction => ValidateNoMissingTransactions(
                bankAccountId,
                newTransactions
                    .Filter(transaction => !existingTransactions.Contains(new(transaction.Date, transaction.Text, transaction.Amount, transaction.Balance))),
                latestTransaction,
                nextBankTransactionId),
            _ => CreateTransactions(bankAccountId, newTransactions, nextBankTransactionId),
        };

    static Either<Failure<ImportError>, Seq<BankTransaction>> ValidateNoMissingTransactions(
        BankAccountId bankAccountId, Seq<ImportBankTransaction> newTransactions, BankTransaction latestTransaction, BankTransactionId nextBankTransactionId) =>
        newTransactions.HeadOrNone().Case switch
        {
            ImportBankTransaction transaction => transaction.Date >= latestTransaction.Date && IsBalanceValid(latestTransaction.Balance, transaction)
                ? CreateTransactions(bankAccountId, newTransactions, nextBankTransactionId)
                : Failure.New(HttpStatusCode.UnprocessableEntity, ImportError.MissingTransactions, "Some transactions are missing."),
            _ => Seq<BankTransaction>(),
        };

    static Seq<BankTransaction> CreateTransactions(BankAccountId bankAccountId, Seq<ImportBankTransaction> transactions, BankTransactionId nextId) =>
        transactions.Tail.Scan(
            CreateTransaction(bankAccountId, nextId, transactions.Head),
            (previousTransaction, transaction) => CreateTransaction(bankAccountId, previousTransaction.BankTransactionId.GetNextId(), transaction));

    static BankTransaction CreateTransaction(BankAccountId bankAccountId, BankTransactionId id, ImportBankTransaction transaction) =>
        new(id, bankAccountId, transaction.Date, transaction.Text, transaction.Amount, transaction.Balance, GetInitialStatus(transaction));

    static BankTransactionStatus GetInitialStatus(ImportBankTransaction transaction) =>
        IsPossiblePayment(transaction) ? BankTransactionStatus.Unknown : BankTransactionStatus.Ignored;

    static bool IsPossiblePayment(ImportBankTransaction transaction) =>
        PaymentIdMatcher.HasPaymentId(transaction.Text) || IsSuspiciousPayIn(transaction);

    static bool IsSuspiciousPayIn(ImportBankTransaction transaction) =>
        transaction.Amount > Amount.Zero &&
        transaction.Amount.ToDecimal()%50 is 0M &&
        !PaymentIdMatcher.HasPaymentId(transaction.Text) &&
        // ReSharper disable once StringLiteralTypo
        !transaction.Reference.Contains("FISAM.POST", StringComparison.Ordinal);

    static Option<LocalDate> GetLatestImportStartDate(Seq<BankTransaction> existingTransactions, Seq<BankTransaction> newTransactions) =>
        newTransactions.IsEmpty
            ? existingTransactions.HeadOrNone().Map(transaction => transaction.Date)
            : newTransactions.HeadOrNone().Map(transaction => transaction.Date);

    static ImportBankTransactionsOutput CreateOutput(
        ImportBankTransactionsInput input,
        Seq<BankTransaction> existingBankTransactionsForAccount,
        Seq<BankTransaction> newTransactions,
        Option<LocalDate> latestImportStartDate) =>
        new(
            newTransactions,
            GetDateRange(existingBankTransactionsForAccount, newTransactions),
            latestImportStartDate,
            GetImport(input.Command.Timestamp, input.Command.BankAccountId, input.ImportOption, newTransactions));

    static Option<DateRange> GetDateRange(Seq<BankTransaction> existingTransactions, Seq<BankTransaction> newTransactions) =>
        (existingTransactions.IsEmpty, newTransactions.IsEmpty) switch
        {
            (false, false) => new DateRange(existingTransactions[0].Date, newTransactions[^1].Date),
            (true, false) => new DateRange(newTransactions[0].Date, newTransactions[^1].Date),
            (false, true) => new DateRange(existingTransactions[0].Date, existingTransactions[^1].Date),
            _ => None,
        };

    static Option<Import> GetImport(Instant timestamp, BankAccountId bankAccountId, Option<Import> importOption, Seq<BankTransaction> transactions) =>
        importOption.Case switch
        {
            Import import => UpdateImport(timestamp, bankAccountId, import, transactions),
            _ => CreateImport(timestamp, bankAccountId, transactions),
        };

    static Import UpdateImport(Instant timestamp, BankAccountId bankAccountId, Import import, Seq<BankTransaction> transactions) =>
        transactions.HeadOrNone().Case switch
        {
            BankTransaction transaction => UpdateImport(timestamp, bankAccountId, import.BankAccounts, transaction.Date),
            _ => import,
        };

    static Import UpdateImport(Instant timestamp, BankAccountId bankAccountId, Seq<BankAccountImport> imports, LocalDate date) =>
        new(timestamp, UpdateBankAccountImports(imports, new(bankAccountId, date), isReplaced: false));

    static Seq<BankAccountImport> UpdateBankAccountImports(Seq<BankAccountImport> imports, BankAccountImport bankAccountImport, bool isReplaced) =>
        imports.HeadOrNone().Case switch
        {
            BankAccountImport import => import.BankAccountId == bankAccountImport.BankAccountId
                ? bankAccountImport.Cons(UpdateBankAccountImports(imports.Tail, bankAccountImport, isReplaced: true))
                : import.Cons(UpdateBankAccountImports(imports.Tail, bankAccountImport, isReplaced)),
            _ when !isReplaced => bankAccountImport.Cons(),
            _ => Empty,
        };

    static Option<Import> CreateImport(Instant timestamp, BankAccountId bankAccountId, Seq<BankTransaction> transactions) =>
        transactions.HeadOrNone().Case switch
        {
            BankTransaction transaction => new Import(timestamp, new BankAccountImport(bankAccountId, transaction.Date).Cons()),
            _ => None,
        };
}
