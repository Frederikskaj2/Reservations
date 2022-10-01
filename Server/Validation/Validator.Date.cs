using Frederikskaj2.Reservations.Application;
using Frederikskaj2.Reservations.Shared.Core;
using LanguageExt;
using NodaTime;
using NodaTime.Text;
using System.Net;
using static LanguageExt.Prelude;

namespace Frederikskaj2.Reservations.Server;

static partial class Validator
{
    static readonly LocalDatePattern datePattern = LocalDatePattern.CreateWithInvariantCulture("yyyy-MM-dd");

    public static Either<Failure, UserReservedDaysCommand> ValidateUserReservedDays(string? fromDate, string? toDate, UserId userId)
    {
        var either = from optionalFromDate in ValidateOptionalDate(fromDate, "From")
            from optionalToDate in ValidateOptionalDate(toDate, "To")
            select new UserReservedDaysCommand(optionalFromDate, optionalToDate, userId);
        return either.MapFailure(HttpStatusCode.UnprocessableEntity);
    }

    public static Either<Failure, ReservedDaysCommand> ValidateReservedDays(string? fromDate, string? toDate)
    {
        var either = from optionalFromDate in ValidateOptionalDate(fromDate, "From")
            from optionalToDate in ValidateOptionalDate(toDate, "To")
            select new ReservedDaysCommand(optionalFromDate, optionalToDate);
        return either.MapFailure(HttpStatusCode.UnprocessableEntity);
    }

    public static Either<Failure, LocalDate> ValidateMonth(string? month)
    {
        var either = from nonEmptyMonth in IsNotNullOrEmpty(month, "Month")
            from date in ValidateDate(nonEmptyMonth, "Month")
            from _ in IsFirstDayOfMonth(date, "Month")
            select date;
        return either.MapFailure(HttpStatusCode.UnprocessableEntity);
    }

    static Either<string, Option<LocalDate>> ValidateOptionalDate(string? date, string context) =>
        date is { Length: > 0 } ? Some(ValidateDate(date, context)).Sequence() : (Option<LocalDate>) None;

    static Either<string, LocalDate> ValidateDate(string date, string context) =>
        datePattern.Parse(date).TryGetValue(default, out var value) ? value : $"{context} is not a valid date.";

    static Either<string, Unit> IsFirstDayOfMonth(LocalDate date, string context) =>
        date.Day == 1 ? unit : $"{context} is not first day of month.";
}
