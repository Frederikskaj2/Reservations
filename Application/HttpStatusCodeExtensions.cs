using System.Net;

namespace Frederikskaj2.Reservations.Application;

public static class HttpStatusCodeExtensions
{
    public static bool IsSuccess(this HttpStatusCode status) => status < (HttpStatusCode) 300;
    public static bool IsServerError(this HttpStatusCode status) => status >= (HttpStatusCode) 500;
}