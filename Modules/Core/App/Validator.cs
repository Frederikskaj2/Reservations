using LanguageExt;
using System.Net;
using System.Text.RegularExpressions;
using static LanguageExt.Prelude;

namespace Frederikskaj2.Reservations.Core;

public static class Validator
{
    public static Either<string, T> HasValue<T>(T? value, string context) where T : notnull =>
        value is not null ? value : $"{context} is missing.";

    public static Either<string, string> IsNotNullOrEmpty(string? value, string context) =>
        value is { Length: > 0 } ? Right(value) : Left($"{context} is missing or empty.");

    public static Either<string, string> IsNotLongerThan(string value, int length, string context) =>
        value.Length <= length ? Right(value) : Left($"{context} exceeds maximum length of {length}.");

    public static Either<string, string> IsMatching(string value, Regex regex, string context) =>
        regex.IsMatch(value) ? Right(value) : Left($"{context} is invalid.");

    public static Either<Failure<Unit>, T> MapFailure<T>(this Either<string, T> self, HttpStatusCode status) =>
        self.MapLeft(details => Failure.New(status, details));

    public static Either<Failure<TFailure>, T> MapFailure<T, TFailure>(this Either<string, T> self, HttpStatusCode status, TFailure failure)
        where TFailure : struct =>
        self.MapLeft(details => Failure.New(status, failure, details));
}
