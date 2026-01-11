using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Persistence;
using Frederikskaj2.Reservations.Users;
using LanguageExt;
using NodaTime;
using System.Threading;
using static Frederikskaj2.Reservations.Bank.PayOutFunctions;
using static Frederikskaj2.Reservations.Persistence.QueryFactory;
using static Frederikskaj2.Reservations.Users.UsersFunctions;
using static LanguageExt.Prelude;

namespace Frederikskaj2.Reservations.Bank;

public static class GetPayOutsShell
{
    static readonly IQuery<PayOut> inCompletePayOutsQuery = Query<PayOut>().Where(payOut => payOut.Status != PayOutStatus.Reconciled).Project();

    static readonly IQuery<PayOut> completePayOutsQuery = Query<PayOut>()
        .Where(payOut => payOut.Status == PayOutStatus.Reconciled)
        .OrderByDescending(payOut => payOut.CreatedTimestamp).Top(5)
        .Project();

    public static EitherAsync<Failure<Unit>, Seq<PayOutSummary>> GetPayOuts(
        IBankingDateProvider bankingDateProvider, IEntityReader reader, ITimeConverter timeConverter, CancellationToken cancellationToken) =>
        from inCompletePayOuts in reader.Query(inCompletePayOutsQuery, cancellationToken).MapReadError()
        from completePayOuts in reader.Query(completePayOutsQuery, cancellationToken).MapReadError()
        from latestImportTimestampOption in ReadLatestImportTimestampOption(reader, cancellationToken)
        let payOuts = inCompletePayOuts.Concat(completePayOuts).OrderByDescending(payOut => payOut.CreatedTimestamp).ToSeq()
        let userIds = toHashSet(payOuts.Map(entity => entity.ResidentId))
        from userExcerpts in ReadUserExcerpts(reader, userIds, cancellationToken)
        let users = toHashMap(userExcerpts.Map(userExcerpt => (userExcerpt.UserId, userExcerpt)))
        select CreatePayOutSummaries(bankingDateProvider, timeConverter, latestImportTimestampOption, payOuts, users);

    static Seq<PayOutSummary> CreatePayOutSummaries(
        IBankingDateProvider bankingDateProvider,
        ITimeConverter timeConverter,
        Option<Instant> latestImportTimestampOption,
        Seq<PayOut> payOuts,
        HashMap<UserId, UserExcerpt> residents) =>
        payOuts.Map(payOut => CreatePayOutSummary(
            payOut,
            residents[payOut.ResidentId],
            GetPayOutDelayedDays(bankingDateProvider, timeConverter, latestImportTimestampOption, payOut)));
}
