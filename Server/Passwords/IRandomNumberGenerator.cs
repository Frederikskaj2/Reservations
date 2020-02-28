namespace Frederikskaj2.Reservations.Server.Passwords
{
    public interface IRandomNumberGenerator
    {
        byte[] CreateRandomBytes(int count);
    }
}