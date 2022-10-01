using Azure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Security.Authentication;

namespace Frederikskaj2.Reservations.Server;

class ExceptionFilter : IExceptionFilter
{
    static readonly IActionResult badRequestResult = new StatusCodeResult(StatusCodes.Status400BadRequest);
    static readonly IActionResult unauthorizedResult = new StatusCodeResult(StatusCodes.Status401Unauthorized);
    static readonly IActionResult forbiddenResult = new StatusCodeResult(StatusCodes.Status403Forbidden);
    static readonly IActionResult notFoundResult = new StatusCodeResult(StatusCodes.Status404NotFound);
    static readonly IActionResult serviceUnavailableResult = new StatusCodeResult(StatusCodes.Status503ServiceUnavailable);
    readonly ILogger logger;

    public ExceptionFilter(ILogger<ExceptionFilter> logger) => this.logger = logger;

    public void OnException(ExceptionContext context)
    {
        var exception = context.Exception is AggregateException aggregateException ? aggregateException.InnerException! : context.Exception;
        var result = GetExceptionResult(exception, context.HttpContext);
        context.Result = result;
        context.ExceptionHandled = true;
    }

    IActionResult GetExceptionResult(Exception exception, HttpContext httpContext)
    {
        logger.LogError(exception, "Exception thrown");
        return exception switch
        {
            BadRequestException _ => badRequestResult,
            AuthenticationException _ => unauthorizedResult,
            UnauthorizedAccessException _ => forbiddenResult,
            NotFoundException _ => notFoundResult,
            HttpRequestException _ => serviceUnavailableResult,
            RequestFailedException _ => serviceUnavailableResult,
            ProblemException problemException => CreateProblemDetailsResult(problemException, httpContext),
            _ => CreateProblemDetailsResult("about:blank", "Internal Server Error", HttpStatusCode.InternalServerError, null, httpContext)
        };
    }

    static ObjectResult CreateProblemDetailsResult(ProblemException problemException, HttpContext httpContext) =>
        CreateProblemDetailsResult(problemException.Type, problemException.Title, problemException.Status, problemException.Detail, httpContext);

    static ObjectResult CreateProblemDetailsResult(string? type, string? title, HttpStatusCode status, string? detail, HttpContext httpContext)
    {
        var problemDetails = new ProblemDetails { Type = type, Title = title, Status = (int) status, Detail = detail };

        var traceId = Activity.Current?.Id ?? httpContext?.TraceIdentifier;
        if (traceId is not null)
            problemDetails.Extensions["traceId"] = traceId;

        var result = new ObjectResult(problemDetails) { StatusCode = (int) status };
        result.ContentTypes.Add("application/problem+json");
        result.ContentTypes.Add("application/problem+xml");
        return result;
    }
}