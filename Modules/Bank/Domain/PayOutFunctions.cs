using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Persistence;
using Frederikskaj2.Reservations.Users;
using LanguageExt;
using NodaTime;
using System.Threading;
using static Frederikskaj2.Reservations.Users.UsersFunctions;
using static LanguageExt.Prelude;

namespace Frederikskaj2.Reservations.Bank;

static class PayOutFunctions
{
    const int bankingDaysBeforePayOutIsConsideredDelayed = 4;

    public static PayOutSummary CreatePayOutSummary(PayOut payOut, UserExcerpt resident, Option<int> delayedDays) =>
        new(
            payOut.PayOutId,
            payOut.CreatedTimestamp,
            resident,
            payOut.AccountNumber,
            payOut.Amount,
            payOut.Status,
            payOut.Resolution,
            delayedDays);

    public static PayOutDetails CreatePayOutDetails(PayOut payOut, UserExcerpt resident, HashMap<UserId, string> userFullNames, Option<int> delayedDays) =>
        new(
            payOut.PayOutId,
            payOut.CreatedTimestamp,
            resident,
            payOut.AccountNumber,
            payOut.Amount,
            payOut.Status,
            payOut.Resolution,
            payOut.Notes,
            payOut.Audits,
            userFullNames,
            delayedDays);

    public static EitherAsync<Failure<Unit>, HashMap<UserId, string>> ReadPayOutUserFullNames(
        IEntityReader reader, PayOut payOut, CancellationToken cancellationToken) =>
        ReadUserFullNamesMap(
            reader,
            toHashSet(payOut.Notes.Map(note => note.UserId).Concat(payOut.Audits.Map(audit => audit.UserId))),
            cancellationToken);

    public static EitherAsync<Failure<Unit>, Option<int>> ReadPayOutDelayedDays(
        IBankingDateProvider bankingDateProvider, IEntityReader reader, ITimeConverter timeConverter, PayOut payOut, CancellationToken cancellationToken) =>
        from latestImportTimestampOption in ReadLatestImportTimestampOption(reader, cancellationToken)
        select GetPayOutDelayedDays(bankingDateProvider, timeConverter, latestImportTimestampOption, payOut);

    public static EitherAsync<Failure<Unit>, Option<Instant>> ReadLatestImportTimestampOption(IEntityReader reader, CancellationToken cancellationToken) =>
        from importOption in reader.Read<Import>(Import.SingletonId, cancellationToken).NotFoundToOption()
        select importOption.Map(import => import.Timestamp);

    public static Option<int> GetPayOutDelayedDays(
        IBankingDateProvider bankingDateProvider, ITimeConverter timeConverter, Option<Instant> latestImportTimestampOption, PayOut payOut) =>
        payOut.Status is PayOutStatus.InProgress
            ? latestImportTimestampOption.Case switch
            {
                Instant latestImportTimestamp => GetPayOutDelayedDays(bankingDateProvider, timeConverter, latestImportTimestamp, payOut.CreatedTimestamp),
                _ => None,
            }
            : None;

    static Option<int> GetPayOutDelayedDays(
        IBankingDateProvider bankingDateProvider, ITimeConverter timeConverter, Instant latestImportTimestamp, Instant payOutTimestamp) =>
        GetPayOutDelayedDays(bankingDateProvider, timeConverter.GetDate(latestImportTimestamp), timeConverter.GetDate(payOutTimestamp));

    static Option<int> GetPayOutDelayedDays(IBankingDateProvider bankingDateProvider, LocalDate latestImportDate, LocalDate payOutDate) =>
        IsPayOutDelayed(bankingDateProvider, latestImportDate, payOutDate)
            ? Period.DaysBetween(payOutDate, latestImportDate)
            : None;

    static bool IsPayOutDelayed(IBankingDateProvider bankingDateProvider, LocalDate latestImportDate, LocalDate payOutDate) =>
        AddBankingDays(bankingDateProvider, payOutDate, bankingDaysBeforePayOutIsConsideredDelayed) <= latestImportDate;

    static LocalDate AddBankingDays(IBankingDateProvider bankingDateProvider, LocalDate date, int count)
    {
        while (count > 0)
        {
            date = date.PlusDays(1);
            if (date.DayOfWeek is IsoDayOfWeek.Saturday or IsoDayOfWeek.Sunday || bankingDateProvider.BankHolidays.Contains(date))
                continue;
            count -= 1;
        }
        return date;
    }
}
