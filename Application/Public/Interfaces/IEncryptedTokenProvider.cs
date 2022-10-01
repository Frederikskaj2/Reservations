using NodaTime;
using System.Collections.Immutable;

namespace Frederikskaj2.Reservations.Application;

public interface IEncryptedTokenProvider
{
    ImmutableArray<byte> CreateToken(string purpose, Instant timestamp, byte[] data);
    TokenValidationResult ValidateToken(string purpose, Instant earliestAcceptableTokenCreationTimestamp, byte[] data, ImmutableArray<byte> token);
}