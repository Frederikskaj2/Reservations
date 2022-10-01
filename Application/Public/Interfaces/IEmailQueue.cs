using Frederikskaj2.Reservations.Shared.Email;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Application;

public interface IEmailQueue
{
    ValueTask Enqueue(Email email);
}
