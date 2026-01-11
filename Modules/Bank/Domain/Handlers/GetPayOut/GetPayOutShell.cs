using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Persistence;
using LanguageExt;
using System.Threading;
using static Frederikskaj2.Reservations.Bank.PayOutFunctions;
using static Frederikskaj2.Reservations.Users.UsersFunctions;

namespace Frederikskaj2.Reservations.Bank;

public static class GetPayOutShell
{
    public static EitherAsync<Failure<Unit>, PayOutDetails> GetPayOut(
        IBankingDateProvider bankingDateProvider,
        IEntityReader reader,
        ITimeConverter timeConverter,
        GetPayOutQuery query,
        CancellationToken cancellationToken) =>
        from payOut in reader.Read<PayOut>(query.PayOutId, cancellationToken).MapReadError()
        from resident in ReadUserExcerpt(reader, payOut.ResidentId, cancellationToken)
        from userFullNames in ReadPayOutUserFullNames(reader, payOut, cancellationToken)
        from delayedDaysOption in ReadPayOutDelayedDays(bankingDateProvider, reader, timeConverter, payOut, cancellationToken)
        select CreatePayOutDetails(payOut, resident, userFullNames, delayedDaysOption);
}
