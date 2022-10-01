using Frederikskaj2.Reservations.Shared.Core;
using Microsoft.Extensions.Options;
using NodaTime;
using System;
using System.Collections.Immutable;
using System.Text;

namespace Frederikskaj2.Reservations.Application;

public class TokenProvider : ITokenProvider
{
    readonly TokenEncryptionOptions tokenEncryptionOptions;
    readonly IEncryptedTokenProvider tokenProvider;

    public TokenProvider(IOptions<TokenEncryptionOptions> tokenEncryptionOptions, IEncryptedTokenProvider tokenProvider)
    {
        this.tokenEncryptionOptions = tokenEncryptionOptions.Value;
        this.tokenProvider = tokenProvider;
    }

    public ImmutableArray<byte> GetConfirmEmailToken(Instant timestamp, UserId userId) =>
        tokenProvider.CreateToken(tokenEncryptionOptions.ConfirmEmailPurpose, timestamp, GetTokenData(userId));

    public TokenValidationResult ValidateConfirmEmailToken(Instant timestamp, UserId userId, ImmutableArray<byte> token) =>
        tokenProvider.ValidateToken(
            tokenEncryptionOptions.ConfirmEmailPurpose,
            timestamp.Minus(tokenEncryptionOptions.ConfirmEmailDuration),
            GetTokenData(userId),
            token);

    public ImmutableArray<byte> GetNewPasswordToken(Instant timestamp, EmailAddress email) =>
        tokenProvider.CreateToken(tokenEncryptionOptions.NewPasswordPurpose, timestamp, GetTokenData(email));

    public TokenValidationResult ValidateNewPasswordToken(Instant timestamp, EmailAddress email, ImmutableArray<byte> token) =>
        tokenProvider.ValidateToken(
            tokenEncryptionOptions.NewPasswordPurpose,
            timestamp.Minus(tokenEncryptionOptions.NewPasswordDuration),
            GetTokenData(email),
            token);

    static byte[] GetTokenData(UserId userId)
    {
        var bytes = new byte[4];
        BitConverter.TryWriteBytes(bytes.AsSpan(), userId.ToInt32());
        return bytes;
    }

    static byte[] GetTokenData(EmailAddress email) => Encoding.UTF8.GetBytes(email.ToString());
}
