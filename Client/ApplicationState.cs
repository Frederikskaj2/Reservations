using System;
using System.Threading.Tasks;
using Frederikskaj2.Reservations.Shared;
using Microsoft.AspNetCore.Components.Authorization;

namespace Frederikskaj2.Reservations.Client
{
    public class ApplicationState
    {
        private readonly AuthenticationStateProvider authenticationStateProvider;
        private AuthenticationState? authenticationState;

        public ApplicationState(AuthenticationStateProvider authenticationStateProvider)
            => this.authenticationStateProvider = authenticationStateProvider
                ?? throw new ArgumentNullException(nameof(authenticationStateProvider));

        public string? RedirectUrl { get; set; }
        public SignUpRequest SignUpRequest { get; private set; } = new SignUpRequest();
        public MyOrder? MyOrder { get; set; }

        public async Task<int?> GetUserId() => (await GetAuthenticationState()).User.Id();

        public async Task<string> GetUserEmail() => (await GetAuthenticationState()).User.Identity.Name;

        public void ResetSignUpState()
        {
            SignUpRequest.Password = string.Empty;
            SignUpRequest.ConfirmPassword = string.Empty;
            SignUpRequest.DidConsent = false;
            SignUpRequest.IsPersistent = false;
        }

        public void ResetStateAfterSignIn()
        {
            authenticationState = null;
            SignUpRequest = new SignUpRequest();
        }

        private async Task<AuthenticationState> GetAuthenticationState()
            => authenticationState ??= await authenticationStateProvider.GetAuthenticationStateAsync();
    }
}