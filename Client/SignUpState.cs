using Frederikskaj2.Reservations.Shared.Web;

namespace Frederikskaj2.Reservations.Client;

public class SignUpState
{
    public SignUpRequest ViewModel { get; private set; } = new();

    public void Reset() => ViewModel = new SignUpRequest();
}