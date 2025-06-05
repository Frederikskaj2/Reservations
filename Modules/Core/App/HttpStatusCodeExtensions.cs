using System.Net;

namespace Frederikskaj2.Reservations.Core;

public static class HttpStatusCodeExtensions
{
    public static bool IsServerError(this HttpStatusCode status) => status >= HttpStatusCode.InternalServerError;
}
