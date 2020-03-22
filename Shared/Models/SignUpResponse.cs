namespace Frederikskaj2.Reservations.Shared
{
    public class SignUpResponse
    {
        public SignUpResult Result { get; set; }
        public AuthenticatedUser? User { get; set; }
    }
}