namespace Frederikskaj2.Reservations.Shared.Web;

public class ConfirmEmailRequest
{
    public string? Email { get; set; }
    public string? Token { get; set; }
}