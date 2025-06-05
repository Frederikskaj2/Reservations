using Microsoft.Extensions.Options;
using NodaTime;
using System.Collections.Immutable;

namespace Frederikskaj2.Reservations.Users;

class TokenValidator(EncryptedTokenProvider encryptedTokenProvider, IOptions<TokenEncryptionOptions> options) : ITokenValidator
{
    readonly TokenEncryptionOptions options = options.Value;

    public TokenValidationResult ValidateConfirmEmailToken(Instant timestamp, UserId userId, ImmutableArray<byte> token) =>
        encryptedTokenProvider.ValidateToken(
            options.ConfirmEmailPurpose,
            timestamp.Minus(options.ConfirmEmailDuration), TokenDataSerializer.Serialize(userId),
            token);

    public TokenValidationResult ValidateNewPasswordToken(Instant timestamp, EmailAddress email, ImmutableArray<byte> token) =>
        encryptedTokenProvider.ValidateToken(
            options.NewPasswordPurpose,
            timestamp.Minus(options.NewPasswordDuration), TokenDataSerializer.Serialize(email),
            token);
}
