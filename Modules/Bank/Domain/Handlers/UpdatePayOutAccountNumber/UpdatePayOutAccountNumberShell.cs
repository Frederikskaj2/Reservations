using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Persistence;
using LanguageExt;
using System.Threading;
using static Frederikskaj2.Reservations.Bank.PayOutFunctions;
using static Frederikskaj2.Reservations.Bank.UpdatePayOutAccountNumber;
using static Frederikskaj2.Reservations.Users.UsersFunctions;
using static LanguageExt.Prelude;

namespace Frederikskaj2.Reservations.Bank;

public static class UpdatePayOutAccountNumberShell
{
    public static EitherAsync<Failure<Unit>, PayOutDetails> UpdatePayOutAccountNumber(
        IBankingDateProvider bankingDateProvider,
        IEntityReader reader,
        ITimeConverter timeConverter,
        IEntityWriter writer,
        UpdatePayOutAccountNumberCommand command,
        CancellationToken cancellationToken) =>
        from payOutEntity in reader.ReadWithETag<PayOut>(command.PayOutId, cancellationToken).MapReadError()
        from resident in ReadUserExcerpt(reader, payOutEntity.Value.ResidentId, cancellationToken)
        from output in UpdatePayOutAccountNumberCore(new(command, payOutEntity.Value)).ToAsync()
        from userFullNames in ReadPayOutUserFullNames(reader, output.PayOut, cancellationToken)
        from delayedDaysOption in ReadPayOutDelayedDays(bankingDateProvider, reader, timeConverter, output.PayOut, cancellationToken)
        from _ in Write(writer, payOutEntity, output, cancellationToken)
        select CreatePayOutDetails(output.PayOut, resident, userFullNames, delayedDaysOption);

    static EitherAsync<Failure<Unit>, Unit> Write(
        IEntityWriter writer, ETaggedEntity<PayOut> payOutEntity, UpdatePayOutAccountNumberOutput output, CancellationToken cancellationToken) =>
        output.IsModified
            ? writer.Write(collector => collector.Add(payOutEntity), tracker => tracker.Update(output.PayOut), cancellationToken).MapWriteError().Map(_ => unit)
            : unit;
}
