namespace Frederikskaj2.Reservations.Server.Passwords
{
    public class PasswordOptions
    {
        public const int TokenEncryptionKeyLength = 32;
        public int PasswordIterationCount { get; set; } = 20000;
        public string? TokenEncryptionKey { get; set; }
    }
}