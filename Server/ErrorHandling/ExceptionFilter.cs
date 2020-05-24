using System;
using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace Frederikskaj2.Reservations.Server.ErrorHandling
{
    public class ExceptionFilter : IExceptionFilter
    {
        private static readonly IActionResult notFoundResult = new StatusCodeResult(404);
        private readonly ILogger logger;

        public ExceptionFilter(ILogger<ExceptionFilter> logger) => this.logger = logger ?? throw new ArgumentNullException(nameof(logger));

        public void OnException(ExceptionContext context)
        {
            if (context is null)
                throw new ArgumentNullException(nameof(context));

            var result = GetExceptionResult(context.Exception, context.HttpContext);
            if (result != null)
            {
                context.Result = result;
                context.ExceptionHandled = true;
            }
        }

        private IActionResult GetExceptionResult(Exception exception, HttpContext httpContext)
        {
            if (exception is null)
                throw new ArgumentNullException(nameof(exception));

            logger.LogWarning(exception, "Exception thrown");
            return exception switch
            {
                ProblemException problemException => CreateProblemDetailsResult(problemException.Type, problemException.Title, problemException.Status, problemException.Detail, httpContext),
                NotFoundException _ => notFoundResult,
                _ => CreateProblemDetailsResult("about:blank", "Internal Server Error", 500, null, httpContext)
            };
        }

        private static ObjectResult CreateProblemDetailsResult(string? type, string? title, int status, string? detail, HttpContext httpContext)
        {
            var problemDetails = new ProblemDetails
            {
                Type = type,
                Title = title,
                Status = status,
                Detail = detail
            };

            var traceId = Activity.Current?.Id ?? httpContext?.TraceIdentifier;
            if (traceId != null)
                problemDetails.Extensions["traceId"] = traceId;

            var result = new ObjectResult(problemDetails)
            {
                StatusCode = status
            };
            result.ContentTypes.Add("application/problem+json");
            result.ContentTypes.Add("application/problem+xml");
            return result;
        }
    }
}
