using Blazored.LocalStorage;
using Frederikskaj2.Reservations.Users;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Client;

public class AuthenticationService(ILocalStorageService localStorageService)
{
    const string accessTokenKeyName = "accessToken";

    public ValueTask<string?> GetAccessToken() => localStorageService.GetItemAsync<string?>(accessTokenKeyName);

    public ValueTask Clear() => SetAccessToken("");

    public ValueTask SetTokens(Tokens tokens) => SetAccessToken(tokens.AccessToken);

    ValueTask SetAccessToken(string accessToken)
        => accessToken is { Length: > 0 }
            ? localStorageService.SetItemAsync(accessTokenKeyName, accessToken)
            : localStorageService.RemoveItemAsync(accessTokenKeyName);
}
