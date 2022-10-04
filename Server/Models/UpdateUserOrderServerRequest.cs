using Frederikskaj2.Reservations.Shared.Web;
using Microsoft.AspNetCore.Mvc;

namespace Frederikskaj2.Reservations.Server;

public class UpdateUserOrderServerRequest
{
    public int OrderId { get; set; }

    [FromBody]
    public UpdateUserOrderRequest? Body { get; set; }
}
