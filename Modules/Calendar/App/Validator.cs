using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Users;
using LanguageExt;
using NodaTime;
using NodaTime.Text;
using System.Net;
using static LanguageExt.Prelude;

namespace Frederikskaj2.Reservations.Calendar;

static class Validator
{
    static readonly LocalDatePattern datePattern = LocalDatePattern.CreateWithInvariantCulture("yyyy-MM-dd");

    public static Either<Failure<Unit>, GetReservedDaysQuery> ValidateGetReservedDays(string? fromDate, string? toDate) =>
        from tuple in ValidateDateRange(fromDate, toDate)
        select new GetReservedDaysQuery(tuple.FromDate, tuple.ToDate);

    public static Either<Failure<Unit>, GetMyReservedDaysQuery> ValidateGetMyReservedDays(string? fromDate, string? toDate, UserId userId) =>
        from tuple in ValidateDateRange(fromDate, toDate)
        select new GetMyReservedDaysQuery(tuple.FromDate, tuple.ToDate, userId);

    public static Either<Failure<Unit>, GetOwnerReservedDaysQuery> ValidateGetOwnerReservedDays(LocalDate today, string? fromDate, string? toDate) =>
        from tuple in ValidateDateRange(fromDate, toDate)
        select new GetOwnerReservedDaysQuery(today, tuple.FromDate, tuple.ToDate);

    static Either<Failure<Unit>, (Option<LocalDate> FromDate, Option<LocalDate> ToDate)> ValidateDateRange(string? fromDate, string? toDate)
    {
        var either =
            from optionalFromDate in ValidateOptionalDate(fromDate, "From")
            from optionalToDate in ValidateOptionalDate(toDate, "To")
            select (optionalFromDate, optionalToDate);
        return either.MapFailure(HttpStatusCode.UnprocessableEntity);
    }

    static Either<string, Option<LocalDate>> ValidateOptionalDate(string? date, string context) =>
        date is { Length: > 0 } ? Some(ValidateDate(date, context)).Sequence() : (Option<LocalDate>) None;

    static Either<string, LocalDate> ValidateDate(string date, string context) =>
        datePattern.Parse(date).TryGetValue(default, out var value) ? value : $"{context} is not a valid date.";
}
