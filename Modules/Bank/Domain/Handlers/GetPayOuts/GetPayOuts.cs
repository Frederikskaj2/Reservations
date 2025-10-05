using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Persistence;
using Frederikskaj2.Reservations.Users;
using LanguageExt;
using NodaTime;
using static LanguageExt.Prelude;

namespace Frederikskaj2.Reservations.Bank;

static class GetPayOuts
{
    const int bankingDaysBeforePayOutIsConsideredStale = 4;

    public static GetPayOutsOutput GetPayOutsCore(IBankingDateProvider bankingDateProvider, ITimeConverter timeConverter, GetPayOutsInput input) =>
        CreateOutput(bankingDateProvider, timeConverter, input, toHashMap(input.UserExcerpts.Map(userExcerpt => (userExcerpt.UserId, userExcerpt))));

    static GetPayOutsOutput CreateOutput(
        IBankingDateProvider bankingDateProvider, ITimeConverter timeConverter, GetPayOutsInput input, HashMap<UserId, UserExcerpt> usersHashMap) =>
        new(input.PayOutEntities.Map(entity => CreatePayOutDetails(bankingDateProvider, timeConverter, input, usersHashMap, entity)));

    static PayOutDetails CreatePayOutDetails(
        IBankingDateProvider bankingDateProvider,
        ITimeConverter timeConverter,
        GetPayOutsInput input,
        HashMap<UserId, UserExcerpt> usersHashMap,
        ETaggedEntity<PayOut> entity) =>
        new(entity.Value, entity.ETag, usersHashMap[entity.Value.UserId],
            GetStaleDays(bankingDateProvider, timeConverter, input.LatestImportTimestamp, entity.Value.Timestamp));

    static Option<int> GetStaleDays(
        IBankingDateProvider bankingDateProvider, ITimeConverter timeConverter, Option<Instant> latestImportTimestampOption, Instant payOutTimestamp) =>
        latestImportTimestampOption.Case switch
        {
            Instant latestImportTimestamp => GetStaleDays(bankingDateProvider, timeConverter, latestImportTimestamp, payOutTimestamp),
            _ => None,
        };

    static Option<int> GetStaleDays(
        IBankingDateProvider bankingDateProvider, ITimeConverter timeConverter, Instant latestImportTimestamp, Instant payOutTimestamp) =>
        GetStaleDays(bankingDateProvider, timeConverter.GetDate(latestImportTimestamp), timeConverter.GetDate(payOutTimestamp));

    static Option<int> GetStaleDays(IBankingDateProvider bankingDateProvider, LocalDate latestImportDate, LocalDate payOutDate) =>
        IsStale(bankingDateProvider, latestImportDate, payOutDate)
            ? Period.DaysBetween(payOutDate, latestImportDate)
            : None;

    static bool IsStale(IBankingDateProvider bankingDateProvider, LocalDate latestImportDate, LocalDate payOutDate)
    {
        var addBankingDays = AddBankingDays(bankingDateProvider, payOutDate, bankingDaysBeforePayOutIsConsideredStale);
        var isStale = addBankingDays <= latestImportDate;
        return isStale;
    }

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
