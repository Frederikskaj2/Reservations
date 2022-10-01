using Frederikskaj2.Reservations.Shared.Web;
using Microsoft.AspNetCore.Mvc;

namespace Frederikskaj2.Reservations.Server;

public class UpdateUserServerRequest
{
    public int UserId { get; set; }

    [FromBody]
    public UpdateUserRequest? Body { get; set; }
}