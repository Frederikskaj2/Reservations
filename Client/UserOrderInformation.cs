using Frederikskaj2.Reservations.Users;

namespace Frederikskaj2.Reservations.Client;

public class UserOrderInformation
{
    public string? FullName { get; set; }
    public string? Phone { get; set; }
    public ApartmentId? ApartmentId { get; set; }
    public string? AccountNumber { get; set; }

    public void Clear()
    {
        FullName = null;
        Phone = null;
        ApartmentId = null;
        AccountNumber = null;
    }
}
