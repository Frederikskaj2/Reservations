using Frederikskaj2.Reservations.Shared.Web;
using Microsoft.AspNetCore.Mvc;

namespace Frederikskaj2.Reservations.Server;

public class ReimburseServerRequest
{
    public int UserId { get; set; }

    [FromBody] public ReimburseRequest? Body { get; set; }
}
