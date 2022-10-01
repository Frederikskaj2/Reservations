using Frederikskaj2.Reservations.Application;
using LanguageExt;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Globalization;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Server;

static class CookieResultExtensions
{
    public static Task<ActionResult<T>> ToResultWithCookieAsync<T>(
        this EitherAsync<Failure, WithCookies<T>> either, ILogger logger, HttpContext httpContext, bool includeFailureDetail = false)
        where T : class
        => either.Match(
            Right: Success,
            Left: failure => Error(failure, logger, httpContext, includeFailureDetail));

    public static Task<ActionResult<T>> ToResultWithCookieAsync<E, T>(
        this EitherAsync<Failure<E>, WithCookies<T>> either, ILogger logger, HttpContext httpContext, bool includeFailureDetail = false)
        where E : struct, IConvertible where T : class
        => either.Match(
            Right: Success,
            Left: failure => Error(failure, logger, httpContext, includeFailureDetail));

    static ActionResult<T> Success<T>(WithCookies<T> withCookies) where T : class => new CookieOkObjectResult(withCookies.Value, withCookies.Cookie);

    static ActionResult Error(Failure failure, ILogger logger, HttpContext httpContext, bool includeFailureDetail)
    {
        LogFailure(failure, logger);
        return ProblemDetailsResult.Create(
            failure.Status,
            includeFailureDetail ? failure.Detail.IfNoneUnsafe((string) null!) : null,
            null,
            httpContext);
    }

    static void LogFailure(Failure failure, ILogger logger) =>
        logger.Log(failure.Status.IsServerError() ? LogLevel.Error : LogLevel.Warning, "Request failed with {Failure}", failure);

    static ActionResult Error<E>(Failure<E> failure, ILogger logger, HttpContext httpContext, bool includeFailureDetail) where E : struct, IConvertible
    {
        LogFailure(failure, logger);
        return ProblemDetailsResult.Create(
            failure.Status,
            includeFailureDetail ? failure.Detail.IfNoneUnsafe((string) null!) : null,
            failure.Value.ToInt32(CultureInfo.InvariantCulture),
            httpContext);
    }

    static void LogFailure<E>(Failure<E> failure, ILogger logger) where E : struct, IConvertible =>
        logger.Log(failure.Status.IsServerError() ? LogLevel.Error : LogLevel.Warning, "Request failed with {Failure}", failure);
}
