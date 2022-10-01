using Frederikskaj2.Reservations.Shared.Core;

namespace Frederikskaj2.Reservations.Client;

public class UserOrderInformation
{
    public string? FullName { get; set; }
    public string? Phone { get; set; }
    public ApartmentId? ApartmentId { get; set; }
    public string? AccountNumber { get; set; }
}
