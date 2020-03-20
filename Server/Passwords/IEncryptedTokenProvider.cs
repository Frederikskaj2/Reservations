using Frederikskaj2.Reservations.Shared;
using NodaTime;

namespace Frederikskaj2.Reservations.Server.Passwords
{
    public interface IEncryptedTokenProvider
    {
        string CreateToken(string purpose, Instant timestamp, byte[] data);
        TokenValidationResult ValidateToken(
            string purpose, Instant earliestAcceptableTokenCreationTimestamp, byte[] data, string token);
    }
}