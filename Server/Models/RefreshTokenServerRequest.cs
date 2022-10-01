using Microsoft.AspNetCore.Mvc;

namespace Frederikskaj2.Reservations.Server;

public class RefreshTokenServerRequest
{
    [FromHeader]
    public string? Cookie { get; set; }
}
