using LanguageExt;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Core;

public static class ResultExtensions
{
    public static Task<IResult> ToResult<TFailure>(
        this EitherAsync<Failure<TFailure>, Unit> either, ILogger logger, HttpContext httpContext, bool includeFailureDetail = false)
        where TFailure : struct
        => either.Match(
            _ => Success(),
            failure => Error(failure, logger, httpContext, includeFailureDetail));

    public static Task<IResult> ToResult<TFailure, TResult>(
        this EitherAsync<Failure<TFailure>, TResult> either, ILogger logger, HttpContext httpContext, bool includeFailureDetail = false)
        where TFailure : struct
        => either.Match(
            Success,
            failure => Error(failure, logger, httpContext, includeFailureDetail));

    public static Task<IResult> ToResult<TFailure, TResult>(
        this EitherAsync<Failure<TFailure>, WithETag<TResult>> either, ILogger logger, HttpContext httpContext, bool includeFailureDetail = false)
        where TFailure : struct
        => either.Match(
            withETag => Success(withETag.Result, httpContext, withETag.ETag),
            failure => Error(failure, logger, httpContext, includeFailureDetail));

    static NoContent Success() => TypedResults.NoContent();

    static Ok<T> Success<T>(T value) => TypedResults.Ok(value);

    static Ok<T> Success<T>(T value, HttpContext httpContext, string? eTag)
    {
        if (eTag is not null)
            httpContext.Response.Headers.ETag = eTag;
        return TypedResults.Ok(value);
    }

    static IResult Error<TFailure>(Failure<TFailure> failure, ILogger logger, HttpContext httpContext, bool includeFailureDetail) where TFailure : struct
    {
        LogFailure(failure, logger);
        return ProblemDetailsResult.Create(
            failure.Status,
            includeFailureDetail ? failure.Detail.ToNullableReference() : null,
            failure.Value.ToString(),
            httpContext);
    }

    static void LogFailure<TFailure>(Failure<TFailure> failure, ILogger logger) where TFailure : struct =>
        logger.Log(failure.Status.IsServerError() ? LogLevel.Error : LogLevel.Warning, "Request failed with {Failure}", failure);
}
