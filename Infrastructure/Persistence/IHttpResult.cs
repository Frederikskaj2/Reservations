using System.Net;

namespace Frederikskaj2.Reservations.Infrastructure.Persistence;

interface IHttpResult
{
    HttpStatusCode Status { get; }
}