using LanguageExt;
using System.Net;
using static LanguageExt.Prelude;

namespace Frederikskaj2.Reservations.Core;

public static class Failure
{
    public static Failure<Unit> New(HttpStatusCode status) => new(status, default);
    public static Failure<Unit> New(HttpStatusCode status, string detail) => new(status, default, detail);
    public static Failure<Unit> New(HttpStatusCode status, Option<string> detail) => new(status, default, detail);

    public static Failure<T> New<T>(HttpStatusCode status, T value) where T : struct => new(status, value);
    public static Failure<T> New<T>(HttpStatusCode status, T value, string detail) where T : struct => new(status, value, detail);
    public static Failure<T> New<T>(HttpStatusCode status, T value, Option<string> detail) where T : struct => new(status, value, detail);
}

public record Failure<T>(HttpStatusCode Status, T Value, Option<string> Detail) where T : struct
{
    internal Failure(HttpStatusCode status, T value) : this(status, value, None) { }
    internal Failure(HttpStatusCode status, T value, string detail) : this(status, value, Some(detail)) { }

    public override string ToString() =>
        (Detail.Case, Value) switch
        {
            (string detail, Unit) => $"{Status}: {detail}",
            (string detail, _) => $"{Status}: {detail} ({Value})",
            (_, Unit) => Status.ToString(),
            _ => $"{Status} ({Value})",
        };
}
