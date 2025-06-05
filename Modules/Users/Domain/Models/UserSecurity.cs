using LanguageExt;
using static LanguageExt.Prelude;

namespace Frederikskaj2.Reservations.Users;

public record UserSecurity
{
    public Seq<byte> HashedPassword { get; init; }
    public TokenId NextRefreshTokenId { get; init; } = TokenId.FromInt32(1);
    public Seq<RefreshToken> RefreshTokens { get; init; } = Empty;
}
