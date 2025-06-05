using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Frederikskaj2.Reservations.Core;

public static class ProblemDetailsResult
{
    public static IResult Create(HttpStatusCode status, string? detail, string? error, HttpContext httpContext)
    {
        var problemDetails = new ProblemDetails { Status = (int) status, Detail = detail };

        if (error is not null)
            problemDetails.Extensions["error"] = error;

        var traceId = System.Diagnostics.Activity.Current?.Id ?? httpContext.TraceIdentifier;
        problemDetails.Extensions["traceId"] = traceId;

        return TypedResults.Problem(problemDetails);
    }
}
