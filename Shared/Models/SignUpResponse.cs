namespace Frederikskaj2.Reservations.Shared
{
    public class SignUpResponse
    {
        public AuthenticatedUser? User { get; set; }
        public SignUpErrorCodes? Errors { get; set; }
    }
}