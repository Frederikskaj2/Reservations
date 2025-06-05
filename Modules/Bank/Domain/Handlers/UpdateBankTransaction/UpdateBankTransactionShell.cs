using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Persistence;
using LanguageExt;
using System.Net;
using System.Threading;
using static Frederikskaj2.Reservations.Bank.UpdateBankTransaction;
using static LanguageExt.Prelude;

namespace Frederikskaj2.Reservations.Bank;

public static class UpdateBankTransactionShell
{
    public static EitherAsync<Failure<Unit>, BankTransaction> UpdateBankTransaction(
        IEntityReader reader, IEntityWriter writer, UpdateBankTransactionCommand command, CancellationToken cancellationToken) =>
        from transactionEntity in reader.ReadWithETag<BankTransaction>(command.BankTransactionId, cancellationToken).MapReadError()
        from _1 in Validate(transactionEntity.Value)
        let output = UpdateBankTransactionCore(new(command, transactionEntity.Value))
        from _2 in WriteIfChanged(writer, transactionEntity, output, cancellationToken)
        select output.Transaction;

    static EitherAsync<Failure<Unit>, Unit> Validate(BankTransaction transaction) =>
        transaction.Status is not BankTransactionStatus.Reconciled
            ? unit
            : Failure.New(HttpStatusCode.UnprocessableEntity, "Cannot change the status of a reconciled transaction.");

    static EitherAsync<Failure<Unit>, Unit> WriteIfChanged(
        IEntityWriter writer, ETaggedEntity<BankTransaction> transactionEntity, UpdateBankTransactionOutput output, CancellationToken cancellationToken) =>
        output.IsChanged
            ? writer
                .Write(collector => collector.Add(transactionEntity), tracker => tracker.Update(output.Transaction), cancellationToken)
                .Map(_ => unit)
                .MapWriteError()
            : unit;
}
