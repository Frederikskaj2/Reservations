using Frederikskaj2.Reservations.Shared.Web;
using Microsoft.AspNetCore.Mvc;

namespace Frederikskaj2.Reservations.Server;

public class UpdateOwnerOrderServerRequest
{
    public int OrderId { get; set; }

    [FromBody]
    public UpdateOwnerOrderRequest? Body { get; set; }
}