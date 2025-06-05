using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Persistence;
using LanguageExt;
using System.Threading;
using static Frederikskaj2.Reservations.LockBox.LockBoxCodesFunctions;

namespace Frederikskaj2.Reservations.LockBox;

public static class GetLockBoxCodesShell
{
    public static EitherAsync<Failure<Unit>, Seq<WeeklyLockBoxCodes>> GetLockBoxCodes(IEntityReader reader, CancellationToken cancellationToken) =>
        from lockBoxCodes in ReadLockBoxCodes(reader, cancellationToken)
        select CreateWeeklyLockBoxCodes(lockBoxCodes);
}
