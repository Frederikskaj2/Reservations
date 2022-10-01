using LanguageExt;
using System;
using System.Net;
using static LanguageExt.Prelude;

namespace Frederikskaj2.Reservations.Application;

public record Failure(HttpStatusCode Status, Option<string> Detail)
{
    public Failure(HttpStatusCode status) : this(status, None) { }
    public Failure(HttpStatusCode status, string detail) : this(status, Some(detail)) { }

    public override string ToString() =>
        Detail.Case switch
        {
            string detail => $"{Status}: {detail}",
            _ => Status.ToString()
        };

    public static Failure New(HttpStatusCode status) => new(status);
    public static Failure New(HttpStatusCode status, string detail) => new(status, detail);
    public static Failure New(HttpStatusCode status, Option<string> detail) => new(status, detail);

    public static Failure<T> New<T>(HttpStatusCode status, T value) where T : struct, IConvertible => new(status, value);
    public static Failure<T> New<T>(HttpStatusCode status, T value, string detail) where T : struct, IConvertible => new(status, value, detail);
    public static Failure<T> New<T>(HttpStatusCode status, T value, Option<string> detail) where T : struct, IConvertible => new(status, value, detail);
}

public record Failure<T>(HttpStatusCode Status, T Value, Option<string> Detail) where T : struct, IConvertible
{

    public override string ToString() =>
        Detail.Case switch
        {
            string detail => $"{Status}: {detail} ({Value})",
            _ => $"{Status} ({Value})"
        };

    public Failure(HttpStatusCode status, T value) : this(status, value, None) { }
    public Failure(HttpStatusCode status, T value, string detail) : this(status, value, Some(detail)) { }
}
