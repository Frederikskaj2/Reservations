using LanguageExt;
using System.Net;
using static LanguageExt.Prelude;

namespace Frederikskaj2.Reservations.Infrastructure.Persistence;

record ETaggedResult<T>(HttpStatusCode Status, Option<ETagged<T>> ETagged) : IHttpResult
{
    public ETaggedResult(HttpStatusCode status) : this(status, None) { }
}
