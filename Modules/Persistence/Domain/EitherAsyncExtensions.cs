using Frederikskaj2.Reservations.Core;
using LanguageExt;
using System.Net;
using static LanguageExt.Prelude;

namespace Frederikskaj2.Reservations.Persistence;

public static class EitherAsyncExtensions
{
    public static EitherAsync<Failure<Unit>, T> MapReadError<T>(this EitherAsync<HttpStatusCode, T> either) =>
        either.MapLeft(status => Failure.New(status.MapReadStatus(), $"Database read error {status}."));

    public static EitherAsync<Failure<TFailure>, T> MapReadError<TFailure, T>(this EitherAsync<HttpStatusCode, T> either) where TFailure : struct =>
        either.MapLeft(status => Failure.New<TFailure>(status.MapReadStatus(), default, $"Database read error {status}."));

    public static EitherAsync<Failure<Unit>, T> MapWriteError<T>(this EitherAsync<HttpStatusCode, T> either) =>
        either.MapLeft(status => Failure.New(status.MapWriteStatus(), $"Database write error {status}."));

    public static EitherAsync<Failure<TFailure>, Unit> MapWriteError<TFailure>(this EitherAsync<HttpStatusCode, Unit> either) where TFailure : struct =>
        either.MapWriteError<Unit, TFailure>();

    public static EitherAsync<Failure<TFailure>, T> MapWriteError<T, TFailure>(this EitherAsync<HttpStatusCode, T> either) where TFailure : struct =>
        either.MapLeft(status => Failure.New<TFailure>(status.MapWriteStatus(), default, $"Database write error {status}."));

    public static EitherAsync<HttpStatusCode, T> NotFoundToForbidden<T>(this EitherAsync<HttpStatusCode, T> either) =>
        either.MapLeft(status => status switch
        {
            HttpStatusCode.NotFound => HttpStatusCode.Forbidden,
            _ => status,
        });

    public static EitherAsync<HttpStatusCode, T> NotFoundToUnprocessableEntity<T>(this EitherAsync<HttpStatusCode, T> either) =>
        either.MapLeft(status => status switch
        {
            HttpStatusCode.NotFound => HttpStatusCode.UnprocessableEntity,
            _ => status,
        });

    public static EitherAsync<Failure<Unit>, Option<T>> NotFoundToOption<T>(this EitherAsync<HttpStatusCode, T> either) =>
        either.BiBind<Option<T>>(
            value => Some(value),
            status => status switch
            {
                HttpStatusCode.NotFound => Option<T>.None,
                _ => status,
            })
            .MapReadError();
}
