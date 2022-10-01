namespace Frederikskaj2.Reservations.Application;

public interface IRandomNumberGenerator
{
    byte[] CreateRandomBytes(int count);
}