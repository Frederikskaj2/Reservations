using System.Net;

namespace Frederikskaj2.Reservations.Infrastructure.Persistence;

static class HttpStatusCodeExtensions
{
    public static bool IsSuccess(this HttpStatusCode status) => status < (HttpStatusCode) 300;
}