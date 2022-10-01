using Blazored.LocalStorage;
using Frederikskaj2.Reservations.Shared.Web;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Client;

public class AuthenticationService
{
    const string accessTokenKeyName = "accessToken";

    readonly ILocalStorageService localStorageService;

    public AuthenticationService(ILocalStorageService localStorageService) => this.localStorageService = localStorageService;

    public ValueTask<string?> GetAccessTokenAsync() => localStorageService.GetItemAsync<string?>(accessTokenKeyName);

    public ValueTask ClearAsync() => SetAccessTokenAsync("");

    public ValueTask SetTokensAsync(Tokens tokens) => SetAccessTokenAsync(tokens.AccessToken);

    ValueTask SetAccessTokenAsync(string accessToken)
        => !string.IsNullOrEmpty(accessToken)
            ? localStorageService.SetItemAsync(accessTokenKeyName, accessToken)
            : localStorageService.RemoveItemAsync(accessTokenKeyName);
}
