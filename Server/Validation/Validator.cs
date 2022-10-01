using Frederikskaj2.Reservations.Application;
using Frederikskaj2.Reservations.Shared.Core;
using LanguageExt;
using NodaTime;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Net;
using System.Text.RegularExpressions;
using static LanguageExt.Prelude;

namespace Frederikskaj2.Reservations.Server;

partial class Validator
{
    static Either<string, string> IsNotNullOrEmpty(string? value, string context) =>
        value is {Length: > 0} ? Right(value) : Left($"{context} is missing or empty.");

    static Either<string, string> IsNotLongerThan(string value, int length, string context) =>
        value.Length <= length ? Right(value) : Left($"{context} exceeds maximum length of {length}.");

    static Either<string, string> IsMatching(string value, Regex regex, string context) =>
        regex.IsMatch(value) ? Right(value) : Left($"{context} is invalid.");

    static Either<string, T> HasValue<T>(T? value, string context) where T : notnull =>
        value is not null ? value : $"{context} is missing.";

    static Either<string, ImmutableArray<byte>> IsBase64(string value, string context)
    {
        try
        {
            return Convert.FromBase64String(value).UnsafeNoCopyToImmutableArray();
        }
        catch (FormatException)
        {
            return $"{context} is not valid base64.";
        }
    }

    static Either<string, int> IsNotLessThan(int value, int lowerBound, string context) =>
        value >= lowerBound ? value : $"{context} is too small.";

    static Either<string, LocalDate> IsNotFutureDate(LocalDate today, LocalDate date, string context) =>
        date <= today ? date : $"{context} is in the future.";

    static Either<string, Amount> ValidateAmount(Amount amount, Amount minimumAmount, Amount maximumAmount, string context)
    {
        if (amount < minimumAmount)
            return $"{context} is too small.";
        if (amount > maximumAmount)
            return $"{context} is too large.";
        return amount;
    }

    static Either<string, Seq<T>> IsBoundedCollection<T>(IEnumerable<T> collection, int minimumItems, int maximumItems, string context)
    {
        var seq = collection.ToSeq();
        if (seq.Count < minimumItems)
            return $"{context} has too few items.";
        if (seq.Count > maximumItems)
            return $"{context} has too many items.";
        return seq;
    }

    static Either<string, Seq<R>> ValidateAll<T, R>(Seq<T> seq, Func<T, Either<string, R>> validator) =>
        seq.Map(validator).Sequence();

    static Either<Failure, T> MapFailure<T>(this Either<string, T> self, HttpStatusCode status) =>
        self.MapLeft(details => Failure.New(status, details));

    static Either<Failure<F>, T> MapFailure<T, F>(this Either<string, T> self, HttpStatusCode status, F failure) where F : struct, IConvertible =>
        self.MapLeft(details => Failure.New(status, failure, details));
}
