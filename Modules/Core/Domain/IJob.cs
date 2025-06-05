using LanguageExt;
using System.Threading;

namespace Frederikskaj2.Reservations.Core;

public interface IJob
{
    EitherAsync<Failure<Unit>, Unit> Invoke(CancellationToken cancellationToken);
}