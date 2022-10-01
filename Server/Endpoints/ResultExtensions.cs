using Frederikskaj2.Reservations.Application;
using LanguageExt;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Globalization;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Server;

static class ResultExtensions
{
    public static Task<ActionResult> ToResultAsync(
        this EitherAsync<Failure, Unit> either, ILogger logger, HttpContext httpContext, bool includeFailureDetail = false)
        => either.Match(
            Right: _ => Success(),
            Left: failure => Error(failure, logger, httpContext, includeFailureDetail));

    public static Task<ActionResult> ToResultAsync<E>(
        this EitherAsync<Failure<E>, Unit> either, ILogger logger, HttpContext httpContext, bool includeFailureDetail = false) where E : struct, IConvertible
        => either.Match(
            Right: _ => Success(),
            Left: failure => Error(failure, logger, httpContext, includeFailureDetail));

    public static Task<ActionResult<T>> ToResultAsync<T>(
        this EitherAsync<Failure, T> either, ILogger logger, HttpContext httpContext, bool includeFailureDetail = false)
        => either.Match(
            Right: Success,
            Left: failure => Error(failure, logger, httpContext, includeFailureDetail));

    public static Task<ActionResult<T>> ToResultAsync<E, T>(
        this EitherAsync<Failure<E>, T> either, ILogger logger, HttpContext httpContext, bool includeFailureDetail = false)
        where E : struct, IConvertible where T : class
        => either.Match(
            Right: Success,
            Left: failure => Error(failure, logger, httpContext, includeFailureDetail));

    static ActionResult Success() => new NoContentResult();

    static ActionResult<T> Success<T>(T value) => new OkObjectResult(value);

    static ActionResult Error(Failure failure, ILogger logger, HttpContext httpContext, bool includeFailureDetail)
    {
        LogFailure(failure, logger);
        return ProblemDetailsResult.Create(failure.Status, includeFailureDetail ? failure.Detail.IfNoneUnsafe((string?) null) : null, null, httpContext);
    }

    static ActionResult Error<E>(Failure<E> failure, ILogger logger, HttpContext httpContext, bool includeFailureDetail) where E : struct, IConvertible
    {
        LogFailure(failure, logger);
        return ProblemDetailsResult.Create(
            failure.Status,
            includeFailureDetail ? failure.Detail.IfNoneUnsafe((string) null!) : null,
            failure.Value.ToInt32(CultureInfo.InvariantCulture),
            httpContext);
    }

    static void LogFailure(Failure failure, ILogger logger) =>
        logger.Log(failure.Status.IsServerError() ? LogLevel.Error : LogLevel.Warning, "Request failed with {Failure}", failure);

    static void LogFailure<E>(Failure<E> failure, ILogger logger) where E : struct, IConvertible =>
        logger.Log(failure.Status.IsServerError() ? LogLevel.Error : LogLevel.Warning, "Request failed with {Failure}", failure);
}
