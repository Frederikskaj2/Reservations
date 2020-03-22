namespace Frederikskaj2.Reservations.Shared
{
    public class SignInResponse
    {
        public SignInResult Result { get; set; }
        public AuthenticatedUser? User { get; set; }
    }
}