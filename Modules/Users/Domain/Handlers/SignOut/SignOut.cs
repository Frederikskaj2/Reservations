using NodaTime;

namespace Frederikskaj2.Reservations.Users;

static class SignOut
{
    public static SignOutOutput SignOutCore(SignOutInput input) =>
        new(RemoveToken(input.Command.Timestamp, input.Command.TokenId, input.User));

    static User RemoveToken(Instant timestamp, TokenId tokenId, User user) =>
        user with
        {
            Security = user.Security with
            {
                RefreshTokens = user.Security.RefreshTokens.Where(token => token.TokenId != tokenId && timestamp <= token.ExpireAt).ToSeq(),
            },
        };
}
