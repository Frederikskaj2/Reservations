namespace Frederikskaj2.Reservations.Client;

public class SignInState
{
    public SignInState(EventAggregator eventAggregator)
    {
        eventAggregator.Subscribe<SignInMessage>(_ => Email = null);
        eventAggregator.Subscribe<SignOutMessage>(_ => Email = null);
    }

    public string? Email { get; set; }
}
