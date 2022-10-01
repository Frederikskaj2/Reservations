namespace Frederikskaj2.Reservations.Shared.Web;

public class SignInRequest
{
    public string? Email { get; set; }
    public string? Password { get; set; }
    public bool IsPersistent { get; set; }
}
