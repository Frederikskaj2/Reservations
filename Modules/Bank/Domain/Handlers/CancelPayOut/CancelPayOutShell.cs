using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Persistence;
using LanguageExt;
using System.Threading;
using static Frederikskaj2.Reservations.Bank.PayOutFunctions;
using static Frederikskaj2.Reservations.Users.UsersFunctions;

namespace Frederikskaj2.Reservations.Bank;

public static class CancelPayOutShell
{
    public static EitherAsync<Failure<Unit>, PayOutDetails>CancelPayOut(
        IBankingDateProvider bankingDateProvider,
        IEntityReader reader,
        ITimeConverter timeConverter,
        IEntityWriter writer,
        CancelPayOutCommand command,
        CancellationToken cancellationToken) =>
        from payOutEntity in reader.ReadWithETag<PayOut>(command.PayOutId, cancellationToken).MapReadError()
        from resident in ReadUserExcerpt(reader, payOutEntity.Value.ResidentId, cancellationToken)
        from inProgressPayOutEntity in reader.ReadWithETag<InProgressPayOut>(resident.UserId, cancellationToken).MapReadError()
        from output in Bank.CancelPayOut.CancelPayOutCore(new(command, payOutEntity.Value)).ToAsync()
        from userFullNames in ReadPayOutUserFullNames(reader, output.PayOut, cancellationToken)
        from delayedDaysOption in ReadPayOutDelayedDays(bankingDateProvider, reader, timeConverter, output.PayOut, cancellationToken)
        from _ in writer.Write(
            collector => collector.Add(payOutEntity).Add(inProgressPayOutEntity),
            tracker => tracker.Update(output.PayOut).Remove(inProgressPayOutEntity),
            cancellationToken).MapWriteError()
        select CreatePayOutDetails(output.PayOut, resident, userFullNames, delayedDaysOption);
}
