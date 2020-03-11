using Frederikskaj2.Reservations.Shared;

namespace Frederikskaj2.Reservations.Client
{
    public class ApplicationState
    {
        public string? RedirectUrl { get; set; }
        public SignUpRequest SignUpRequest { get; private set; } = new SignUpRequest();

        public void ResetSignUpState()
        {
            SignUpRequest.Password = string.Empty;
            SignUpRequest.ConfirmPassword = string.Empty;
            SignUpRequest.DidConsent = false;
            SignUpRequest.IsPersistent = false;
        }

        public void ResetStateAfterSignIn() => SignUpRequest = new SignUpRequest();
    }
}