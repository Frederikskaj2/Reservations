using Frederikskaj2.Reservations.Shared.Web;
using Microsoft.AspNetCore.Mvc;

namespace Frederikskaj2.Reservations.Server;

public class SettleReservationServerRequest
{
    public int OrderId { get; set; }

    [FromBody]
    public SettleReservationRequest? Body { get; set; }
}