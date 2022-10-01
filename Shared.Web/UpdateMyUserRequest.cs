using Frederikskaj2.Reservations.Shared.Core;

namespace Frederikskaj2.Reservations.Shared.Web;

public class UpdateMyUserRequest
{
    public string? FullName { get; set; }
    public string? Phone { get; set; }
    public EmailSubscriptions EmailSubscriptions { get; set; }
}
