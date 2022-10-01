using Frederikskaj2.Reservations.Shared.Core;

namespace Frederikskaj2.Reservations.Shared.Web;

public class SignUpRequest
{
    public string? Email { get; set; }
    public string? FullName { get; set; }
    public string? Phone { get; set; }
    public ApartmentId? ApartmentId { get; set; }
    public string? Password { get; set; }
}
