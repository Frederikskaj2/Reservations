using System.Net;

namespace Frederikskaj2.Reservations.Persistence;

static class HttpStatusCodeExtensions
{
    public static bool IsSuccess(this HttpStatusCode status) => status < HttpStatusCode.Ambiguous;
}
