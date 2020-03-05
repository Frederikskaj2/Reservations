namespace Frederikskaj2.Reservations.Shared
{
    public class SignInResponse
    {
        public SignInResult Result { get; set; }
        public UserInfo? User { get; set; }
    }
}