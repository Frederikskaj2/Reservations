using Frederikskaj2.Reservations.Shared.Web;
using Microsoft.AspNetCore.Mvc;

namespace Frederikskaj2.Reservations.Server;

public class UpdateUserReservationsServerRequest
{
    public int OrderId { get; set; }

    [FromBody]
    public UpdateUserReservationsRequest? Body { get; set; }
}
