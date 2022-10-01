using Frederikskaj2.Reservations.Shared.Web;
using Microsoft.AspNetCore.Mvc;

namespace Frederikskaj2.Reservations.Server;

public class SignInServerRequest
{
    [FromHeader]
    public string? Cookie { get; set; }

    [FromBody]
    public SignInRequest? ClientRequest { get; set; }
}
