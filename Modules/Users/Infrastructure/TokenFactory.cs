using Microsoft.Extensions.Options;
using NodaTime;
using System.Collections.Immutable;

namespace Frederikskaj2.Reservations.Users;

class TokenFactory(EncryptedTokenProvider encryptedTokenProvider, IOptions<TokenEncryptionOptions> options)
{
    readonly TokenEncryptionOptions options = options.Value;

    public ImmutableArray<byte> GetConfirmEmailToken(Instant timestamp, UserId userId) =>
        encryptedTokenProvider.CreateToken(options.ConfirmEmailPurpose, timestamp, TokenDataSerializer.Serialize(userId));

    public ImmutableArray<byte> GetNewPasswordToken(Instant timestamp, EmailAddress email) =>
        encryptedTokenProvider.CreateToken(options.NewPasswordPurpose, timestamp, TokenDataSerializer.Serialize(email));
}
