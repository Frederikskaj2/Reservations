using Frederikskaj2.Reservations.Shared.Web;
using Microsoft.AspNetCore.Mvc;

namespace Frederikskaj2.Reservations.Server;

public class UpdatePasswordServerRequest
{
    [FromHeader]
    public string? Cookie { get; set; }

    [FromBody]
    public UpdatePasswordRequest? ClientRequest { get; set; }
}
