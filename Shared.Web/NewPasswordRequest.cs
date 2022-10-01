namespace Frederikskaj2.Reservations.Shared.Web;

public class NewPasswordRequest
{
    public string? Email { get; set; }
    public string? Token { get; set; }
    public string? NewPassword { get; set; }
}