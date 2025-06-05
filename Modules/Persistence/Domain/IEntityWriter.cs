using Frederikskaj2.Reservations.Core;
using LanguageExt;
using System.Net;
using System.Threading;

namespace Frederikskaj2.Reservations.Persistence;

public interface IEntityWriter
{
    EitherAsync<HttpStatusCode, Seq<(EntityOperation Operation, ETag ETag)>> Write(Seq<EntityOperation> operations, CancellationToken cancellationToken);
}
