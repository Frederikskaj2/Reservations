using Frederikskaj2.Reservations.Core;
using LanguageExt;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Users;

static class CookieResultExtensions
{
    public static Task<IResult> ToResultWithCookie<TFailure, TResult>(
        this EitherAsync<Failure<TFailure>, WithCookies<TResult>> either, ILogger logger, HttpContext httpContext, bool includeFailureDetail = false)
        where TFailure : struct where TResult : class
        => either.Match(
            Success,
            failure => Error(failure, logger, httpContext, includeFailureDetail));

    static OkWithCookies<TResult> Success<TResult>(WithCookies<TResult> withCookies) where TResult : class =>
        new(withCookies.Value, withCookies.Cookies);

    static IResult Error<TFailure>(Failure<TFailure> failure, ILogger logger, HttpContext httpContext, bool includeFailureDetail)
        where TFailure : struct
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
