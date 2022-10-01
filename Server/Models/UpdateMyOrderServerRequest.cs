using Frederikskaj2.Reservations.Shared.Web;
using Microsoft.AspNetCore.Mvc;

namespace Frederikskaj2.Reservations.Server;

public class UpdateMyOrderServerRequest
{
    public int OrderId { get; set; }

    [FromBody]
    public UpdateMyOrderRequest? Body { get; set; }
}