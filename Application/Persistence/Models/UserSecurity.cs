using Frederikskaj2.Reservations.Shared.Core;
using LanguageExt;
using static LanguageExt.Prelude;

namespace Frederikskaj2.Reservations.Application;

record UserSecurity
{
    public Seq<byte> HashedPassword { get; init; }
    public TokenId NextRefreshTokenId { get; init; } = TokenId.FromInt32(1);
    public Seq<RefreshToken> RefreshTokens { get; init; } = Empty;
}
