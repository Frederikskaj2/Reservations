using LanguageExt;

namespace Frederikskaj2.Reservations.Application;

public interface IPasswordHasher
{
    Seq<byte> HashPassword(string password);
    PasswordVerificationResult VerifyHashedPassword(Seq<byte> hashedPassword, string providedPassword);
}