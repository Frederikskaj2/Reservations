using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Mime;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Users;

sealed partial class OkWithCookies<TValue>(TValue? value, IEnumerable<Cookie> cookies)
    : IResult, IEndpointMetadataProvider, IStatusCodeHttpResult, IValueHttpResult, IValueHttpResult<TValue>
{
    const int statusCode = StatusCodes.Status200OK;

    static void IEndpointMetadataProvider.PopulateMetadata(MethodInfo method, EndpointBuilder builder) =>
        builder.Metadata.Add(new ProducesResponseTypeMetadata());

    public Task ExecuteAsync(HttpContext httpContext)
    {
        var loggerFactory = httpContext.RequestServices.GetRequiredService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger<OkWithCookies<TValue>>();
        Log.WritingResultAsStatusCode(logger, statusCode);

        httpContext.Response.StatusCode = statusCode;

        foreach (var cookie in cookies)
        {
            var options = new CookieOptions
            {
                Secure = true,
                HttpOnly = true,
                SameSite = SameSiteMode.Strict,
                MaxAge = cookie.MaxAge?.ToTimeSpan(),
                Path = "/",
            };
            httpContext.Response.Cookies.Append(cookie.Name, cookie.Value, options);
        }

        return WriteResultAsJson(httpContext, logger, Value);
    }

    int? IStatusCodeHttpResult.StatusCode => statusCode;

    object? IValueHttpResult.Value => Value;

    public TValue? Value { get; } = value;

    static Task WriteResultAsJson(
        HttpContext httpContext, ILogger logger, TValue? value, string? contentType = null, JsonSerializerOptions? jsonSerializerOptions = null)
    {
        if (value is null)
            return Task.CompletedTask;

        var declaredType = typeof(TValue);
        if (declaredType.IsValueType)
        {
            Log.WritingResultAsJson(logger, declaredType.Name);
            return httpContext.Response.WriteAsJsonAsync(value, jsonSerializerOptions, contentType, httpContext.RequestAborted);
        }

        var runtimeType = value.GetType();
        Log.WritingResultAsJson(logger, runtimeType.Name);
        return httpContext.Response.WriteAsJsonAsync(value, runtimeType, jsonSerializerOptions, contentType, httpContext.RequestAborted);
    }

    static partial class Log
    {
        [LoggerMessage(1, LogLevel.Information, "Setting HTTP status code {StatusCode}.", EventName = "WritingResultAsStatusCode")]
        public static partial void WritingResultAsStatusCode(ILogger logger, int statusCode);

        [LoggerMessage(3, LogLevel.Information, "Writing value of type '{Type}' as Json.", EventName = "WritingResultAsJson")]
        public static partial void WritingResultAsJson(ILogger logger, string type);
    }

    sealed class ProducesResponseTypeMetadata : IProducesResponseTypeMetadata
    {
        public Type? Type { get; } = typeof(TValue);
        public int StatusCode => StatusCodes.Status200OK;
        public IEnumerable<string> ContentTypes { get; } = [MediaTypeNames.Application.Json];
    }
}
