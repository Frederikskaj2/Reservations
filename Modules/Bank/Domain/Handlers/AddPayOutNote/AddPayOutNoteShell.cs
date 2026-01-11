using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Persistence;
using LanguageExt;
using System.Threading;
using static Frederikskaj2.Reservations.Bank.AddPayOutNote;
using static Frederikskaj2.Reservations.Bank.PayOutFunctions;
using static Frederikskaj2.Reservations.Users.UsersFunctions;

namespace Frederikskaj2.Reservations.Bank;

public static class AddPayOutNoteShell
{
    public static EitherAsync<Failure<Unit>, PayOutDetails> AddPayOutNote(
        IBankingDateProvider bankingDateProvider,
        IEntityReader reader,
        ITimeConverter timeConverter,
        IEntityWriter writer,
        AddPayOutNoteCommand command,
        CancellationToken cancellationToken) =>
        from payOutEntity in reader.ReadWithETag<PayOut>(command.PayOutId, cancellationToken).MapReadError()
        from resident in ReadUserExcerpt(reader, payOutEntity.Value.ResidentId, cancellationToken)
        let output = AddPayOutNoteCore(new(command, payOutEntity.Value))
        from userFullNames in ReadPayOutUserFullNames(reader, output.PayOut, cancellationToken)
        from delayedDaysOption in ReadPayOutDelayedDays(bankingDateProvider, reader, timeConverter, output.PayOut, cancellationToken)
        from _ in writer.Write(collector => collector.Add(payOutEntity), tracker => tracker.Update(output.PayOut), cancellationToken).MapWriteError()
        select CreatePayOutDetails(output.PayOut, resident, userFullNames, delayedDaysOption);
}
