using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Globalization;
using System.Net;

namespace Frederikskaj2.Reservations.Server;

static class ProblemDetailsResult
{
    public static ObjectResult Create(HttpStatusCode status, string? detail, int? error, HttpContext httpContext)
    {
        var problemDetails = new ProblemDetails
        {
            Status = (int) status,
            Detail = detail
        };

        if (error.HasValue)
            problemDetails.Extensions["error"] = error.Value.ToString(CultureInfo.InvariantCulture);

        var traceId = Activity.Current?.Id ?? httpContext.TraceIdentifier;
        problemDetails.Extensions["traceId"] = traceId;

        var result = new ObjectResult(problemDetails)
        {
            StatusCode = (int) status
        };
        result.ContentTypes.Add("application/problem+json");
        result.ContentTypes.Add("application/problem+xml");
        return result;
    }
}
