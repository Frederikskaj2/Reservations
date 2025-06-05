using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Persistence;
using LanguageExt;
using System.Threading;
using static LanguageExt.Prelude;

namespace Frederikskaj2.Reservations.Bank;

public static class DeletePayOutShell
{
    public static EitherAsync<Failure<Unit>, Unit> DeletePayOut(IEntityWriter writer, DeletePayOutCommand command, CancellationToken cancellationToken) =>
        from _ in writer.Write(tracker => tracker.Remove<PayOut>(command.PayOutId, command.Etag), cancellationToken).MapWriteError()
        select unit;
}
