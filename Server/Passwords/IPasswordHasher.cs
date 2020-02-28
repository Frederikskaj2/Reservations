namespace Frederikskaj2.Reservations.Server.Passwords
{
    public interface IPasswordHasher
    {
        string HashPassword(string password);
        PasswordVerificationResult VerifyHashedPassword(string hashedPassword, string providedPassword);
    }
}