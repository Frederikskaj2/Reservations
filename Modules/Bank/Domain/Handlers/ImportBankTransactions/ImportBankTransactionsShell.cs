using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Persistence;
using LanguageExt;
using System.Threading;
using static Frederikskaj2.Reservations.Bank.ImportBankTransactions;
using static Frederikskaj2.Reservations.Persistence.QueryFactory;
using static LanguageExt.Prelude;

namespace Frederikskaj2.Reservations.Bank;

public static class ImportBankTransactionsShell
{
    public static EitherAsync<Failure<ImportError>, ImportResult> ImportBankTransactions(
        IBankTransactionsParser bankTransactionsParser,
        IEntityReader reader,
        IEntityWriter writer,
        ImportBankTransactionsCommand command,
        CancellationToken cancellationToken) =>
        from newTransactions in bankTransactionsParser.ParseBankTransactions(command.Transactions).ToAsync()
        from existingTransactions in ReadExistingTransactions(reader, cancellationToken)
        from importOption in ReadImport(reader, cancellationToken)
        from output in ImportBankTransactionsCore(new(command, newTransactions, existingTransactions, importOption)).ToAsync()
        from _ in writer.Write(tracker => TrackEntities(output, tracker), cancellationToken).Map(_ => unit).MapWriteError<ImportError>()
        select new ImportResult(output.Transactions.Count, output.DateRange, output.LatestImportStartDate);

    static EitherAsync<Failure<ImportError>, Seq<BankTransaction>> ReadExistingTransactions(IEntityReader reader, CancellationToken cancellationToken) =>
        reader
            .Query(Query<BankTransaction>().OrderBy(transaction => transaction.BankTransactionId).Project(), cancellationToken)
            .MapReadError<ImportError, Seq<BankTransaction>>();

    static EitherAsync<Failure<ImportError>, Option<Import>> ReadImport(IEntityReader reader, CancellationToken cancellationToken) =>
        reader.Read<Import>(Import.SingletonId, cancellationToken).NotFoundToOption<ImportError, Import>();

    static EntityTracker TrackEntities(ImportBankTransactionsOutput output, EntityTracker tracker) =>
        TryUpsertImportEntity(output, tracker.Add(output.Transactions));

    static EntityTracker TryUpsertImportEntity(ImportBankTransactionsOutput output, EntityTracker tracker) =>
        output.Import.Case switch
        {
            Import import => tracker.Upsert(import),
            _ => tracker,
        };
}
