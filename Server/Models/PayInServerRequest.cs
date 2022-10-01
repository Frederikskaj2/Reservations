using Frederikskaj2.Reservations.Shared.Web;
using Microsoft.AspNetCore.Mvc;

namespace Frederikskaj2.Reservations.Server;

public class PayInServerRequest
{
    public string? PaymentId { get; set; }

    [FromBody]
    public PayInRequest? ClientRequest { get; set; }
}