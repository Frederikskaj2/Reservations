using Frederikskaj2.Reservations.Shared.Web;
using Microsoft.AspNetCore.Mvc;

namespace Frederikskaj2.Reservations.Server;

public class PayOutServerRequest
{
    public int UserId { get; set; }

    [FromBody]
    public PayOutRequest? Body { get; set; }
}