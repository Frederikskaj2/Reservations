using System.Net;

namespace Frederikskaj2.Reservations.Application;

public record CosmosResult(HttpStatusCode Status)
{
    public bool IsSuccess => Status < (HttpStatusCode) 300;
}