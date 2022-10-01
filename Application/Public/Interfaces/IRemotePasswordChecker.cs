using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Application;

public interface IRemotePasswordChecker
{
    Task<int> GetPasswordExposedCount(string password);
}