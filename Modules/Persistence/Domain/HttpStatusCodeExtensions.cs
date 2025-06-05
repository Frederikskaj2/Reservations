using System.Net;

namespace Frederikskaj2.Reservations.Persistence;

static class HttpStatusCodeExtensions
{
    public static HttpStatusCode MapReadStatus(this HttpStatusCode status) =>
        status switch
        {
            HttpStatusCode.NotFound => status,
            _ => HttpStatusCode.ServiceUnavailable,
        };

    public static HttpStatusCode MapNotFoundReadStatusToForbidden(this HttpStatusCode status) =>
        status switch
        {
            HttpStatusCode.NotFound => HttpStatusCode.Forbidden,
            _ => HttpStatusCode.ServiceUnavailable,
        };

    public static HttpStatusCode MapWriteStatus(this HttpStatusCode status) =>
        status switch
        {
            HttpStatusCode.NotFound => status, // Replace or delete with wrong ID.
            HttpStatusCode.Conflict => status, // Create with existing ID.
            HttpStatusCode.PreconditionFailed => status, // Replace or delete with unmatched ETag.
            _ => HttpStatusCode.ServiceUnavailable,
        };
}
